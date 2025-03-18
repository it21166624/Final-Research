
using Egg_Pedict_BackEnd.Data;
using Microsoft.EntityFrameworkCore;

namespace Egg_Pedict_BackEnd.Services
{
    public class HenAgeUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HenAgeUpdaterService> _logger;

        public HenAgeUpdaterService(IServiceProvider serviceProvider, ILogger<HenAgeUpdaterService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HenAgeUpdaterService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateHenAgesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating hen ages.");
                }

                // Run every 24 hours (adjust as needed)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task UpdateHenAgesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var hens = await dbContext.LiveData.ToListAsync();

                foreach (var hen in hens)
                {
                    hen.Hen_Age_weeks = Math.Round((DateTime.UtcNow - hen.Born_Date).TotalDays / 7.0, 2);
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Hen ages updated successfully.");
            }
        }
    }
}
