using Egg_Pedict_BackEnd.Data;
using Egg_Pedict_BackEnd.Model;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

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
                .WithTcpServer("localhost", 1883) 
                .WithClientId("EggPredictionClient")
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Connected to MQTT Broker!");
                await _mqttClient.SubscribeAsync("poultry/heartBeat");
                await _mqttClient.SubscribeAsync("poultry/light_status");

                _logger.LogInformation("Subscribed to topics: sensor/data & poultry/light_status");
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
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogInformation($"Message Received from Topic [{topic}]: {payload}");

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var LiveData = JsonConvert.DeserializeObject<LiveData>(payload);

                if (LiveData != null)
                {
                    var existingRecord = await dbContext.LiveData.FirstOrDefaultAsync(x => x.Id == 1);

                    if (existingRecord != null)
                    {

                        existingRecord.Light_Hours = LiveData.Light_Hours_Sec > 0 ? Math.Round(LiveData.Light_Hours_Sec / 3600.0, 2) : existingRecord.Light_Hours;
                        existingRecord.Temperature = LiveData.Temperature > 0 ? LiveData.Temperature : existingRecord.Temperature;
                        existingRecord.Humidity = LiveData.Humidity > 0 ? LiveData.Humidity : existingRecord.Humidity;
                        existingRecord.Hen_Age_weeks = LiveData.Hen_Age_weeks > 0 ? LiveData.Hen_Age_weeks : existingRecord.Hen_Age_weeks;
                        existingRecord.Feed_Quantity = LiveData.Feed_Quantity > 0 ? LiveData.Feed_Quantity : existingRecord.Feed_Quantity;
                        existingRecord.Hen_Count = LiveData.Hen_Count > 0 ? LiveData.Hen_Count : existingRecord.Hen_Count;
                        existingRecord.Egg_count = LiveData.Egg_count > 0 ? LiveData.Egg_count : existingRecord.Egg_count;
                        existingRecord.HeartRate = LiveData.HeartRate > 0 ? LiveData.HeartRate : existingRecord.HeartRate;
                        existingRecord.BodyTemp = LiveData.BodyTemp > 0 ? LiveData.BodyTemp : existingRecord.BodyTemp;
                        existingRecord.Health_Status = LiveData.Health_Status ?? existingRecord.Health_Status;

                        dbContext.LiveData.Update(existingRecord);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("Sensor Live Data Updated Successfully.");
                    }
                    else
                    {
                        _logger.LogWarning("No existing record found with Id 1.");
                    }
                }
                else
                {
                    _logger.LogWarning("Deserialized LiveData is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling message: {ex.Message}");
            }
        }
    }
}
