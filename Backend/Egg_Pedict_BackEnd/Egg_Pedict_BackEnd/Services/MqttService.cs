using Egg_Pedict_BackEnd.Data;
using Egg_Pedict_BackEnd.Model;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using System.Text;

namespace Egg_Pedict_BackEnd.Services
{
    public class MqttService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MqttService> _logger;
        private IMqttClient _mqttClient;

        public MqttService(IServiceProvider serviceProvider, ILogger<MqttService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Service...");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883) // Connect to Mosquitto broker
                .WithClientId("EggPredictionClient")
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Connected to MQTT Broker!");
                await _mqttClient.SubscribeAsync("sensor/data");
                _logger.LogInformation("📡 Subscribed to topic: sensor/data");
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("Disconnected from MQTT Broker. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                await _mqttClient.ConnectAsync(options, cancellationToken);
            };

            await _mqttClient.ConnectAsync(options, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Service...");
            var disconnectOptions = new MqttClientDisconnectOptions();
            await _mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
        }

        private async Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogInformation($"Message Received: {payload}");

                // Parse sensor data
                var sensorData = JsonConvert.DeserializeObject<SensorData>(payload);

                if (sensorData != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await dbContext.SensorData.AddAsync(sensorData);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Data Saved: {sensorData.Temperature}°C, {sensorData.Humidity}%");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling message: {ex.Message}");
            }
        }
    }
}
