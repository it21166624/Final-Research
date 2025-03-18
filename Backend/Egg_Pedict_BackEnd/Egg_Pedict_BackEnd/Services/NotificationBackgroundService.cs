
using System.Net.Http.Headers;

namespace Egg_Pedict_BackEnd.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(ILogger<NotificationBackgroundService> logger)
        {
            _logger = logger;
        }
        protected override async  Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //await SendNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending notifications.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every 1 minute
            }
        }
        private async Task SendNotificationsAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {

                    client.BaseAddress = new Uri("https://app.newsletters.lk/smsAPI?sendsms");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string apiKey = "R9DVWVFrN9vG588lzhmKM756LxEK3cNy";
                    string authToken = "V4Et1711607434";
                    string from = "MY CAR WASH";
                    string to = "0712775742";

                    Random random = new Random();
                    string pinCode = (random.Next() % 90000 + 10000).ToString();

                    HttpResponseMessage httpResponse = await client.GetAsync(client.BaseAddress + "& apikey=" + apiKey + "& apitoken=" + authToken + "& type=sms&from=" + from + "& to=" + to + "& text=" + pinCode + "&route=0");
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("✅ Notification Send Successfully.");

                    }
                    else
                    {
                        _logger.LogError( httpResponse.ReasonPhrase);
                    }

                }
                catch (FileNotFoundException e)
                {
                    _logger.LogError(e.Message, "Error occurred while sending notifications.");
                }


            }
        }
    }
}
