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
                .WithTcpServer("localhost", 1883) // Change this if using an external broker
                .WithClientId("EggPredictionClient")
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("Connected to MQTT Broker!");

                // Subscribe to both topics (sensor data + LDR data)
                await _mqttClient.SubscribeAsync("sensor/data");
                await _mqttClient.SubscribeAsync("poultry/light_status");

                _logger.LogInformation("📡 Subscribed to topics: sensor/data & poultry/light_status");
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
                        var ldrReadings = await dbContext.LDRData
                            .OrderBy(r => r.Timestamp)
                            .ToListAsync();

                        var lightHours = ldrReadings
                            .GroupBy(r => r.Timestamp.Date)
                            .Select(g =>
                            {
                                var periods = g.OrderBy(r => r.Timestamp).ToList();
                                double totalSeconds = 0;

                                for (int i = 1; i < periods.Count; i++)
                                {
                                    if (periods[i - 1].LightStatus == "ON")
                                    {
                                        totalSeconds += (periods[i].Timestamp - periods[i - 1].Timestamp).TotalSeconds;
                                    }
                                }

                                return new
                                {
                                    Date = g.Key,
                                    LightHours = totalSeconds / 3600.0 // Convert seconds to hours
                                };
                            })
                            .ToList();

                        existingRecord.Light_Hours = lightHours.Sum(lh => lh.LightHours);
                        existingRecord.Temperature = LiveData.Temperature; 
                        existingRecord.Humidity = LiveData.Humidity; 
                        existingRecord.Hen_Age_weeks = LiveData.Hen_Age_weeks; 
                        existingRecord.Feed_Quantity = LiveData.Feed_Quantity;
                        existingRecord.Hen_Count = LiveData.Hen_Count;
                        existingRecord.Egg_count = LiveData.Egg_count;
                        existingRecord.Health_Status = LiveData.Health_Status;


                        dbContext.LiveData.Update(existingRecord);
                        _logger.LogInformation("🔄 Sensor Live Data Updated Successfully.");
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No existing record found with Id 1.");
                    }
                }
                if (LiveData != null)
                {
                    var existingRecord = await dbContext.LiveData.FirstOrDefaultAsync(x => x.Id == 1);

                    if (existingRecord != null)
                    {
                        var ldrReadings = await dbContext.LDRData
                            .OrderBy(r => r.Timestamp)
                            .ToListAsync();

                        var lightHours = ldrReadings
                            .GroupBy(r => r.Timestamp.Date)
                            .Select(g =>
                            {
                                var periods = g.OrderBy(r => r.Timestamp).ToList();
                                double totalSeconds = 0;

                                for (int i = 1; i < periods.Count; i++)
                                {
                                    if (periods[i - 1].LightStatus == "ON")
                                    {
                                        totalSeconds += (periods[i].Timestamp - periods[i - 1].Timestamp).TotalSeconds;
                                    }
                                }

                                return new
                                {
                                    Date = g.Key,
                                    LightHours = totalSeconds / 3600.0 // Convert seconds to hours
                                };
                            })
                            .ToList();

                        existingRecord.Light_Hours = lightHours.Sum(lh => lh.LightHours);

                        dbContext.LiveData.Update(existingRecord);
                        _logger.LogInformation("🔄 Sensor Live Data Updated Successfully.");
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No existing record found with Id 1.");
                    }
                }


                if (topic == "sensor/data")
                {
                    // Process General Sensor Data
                    var sensorData = JsonConvert.DeserializeObject<SenserDataNew>(payload);
                    if (sensorData != null)
                    {
                        await dbContext.senserDataNew.AddAsync(sensorData);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("✅ Sensor Data Saved Successfully.");
                    }
                }
                else if (topic == "poultry/light_status")
                {
                    // Process LDR Data Separately
                    var ldrData = JsonConvert.DeserializeObject<LDRDataDto>(payload);
                    if (ldrData != null)
                    {
                        string lightStatus = ldrData.LDRValue > 700 ? "ON" : "OFF";

                        var newLDRRecord = new LDRData
                        {
                            Timestamp = DateTime.UtcNow,
                            LDRValue = ldrData.LDRValue,
                            LightStatus = lightStatus
                        };

                        await dbContext.LDRData.AddAsync(newLDRRecord);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"✅ LDR Data Saved: {newLDRRecord.LDRValue}, Status: {newLDRRecord.LightStatus}");
                    }
                }
                else
                {
                    _logger.LogWarning($"⚠️ Unhandled topic: {topic}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error handling message: {ex.Message}");
            }
        }

        private class LDRDataDto
        {
            public int LDRValue { get; set; }
        }
    }
}
