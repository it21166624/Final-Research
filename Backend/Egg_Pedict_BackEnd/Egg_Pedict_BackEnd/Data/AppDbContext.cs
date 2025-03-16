using Egg_Pedict_BackEnd.Model;
using Microsoft.EntityFrameworkCore;

namespace Egg_Pedict_BackEnd.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<SensorData> SensorData { get; set; }
        public DbSet<SenserDataNew> senserDataNew { get; set; }
        public DbSet<LDRData> LDRData { get; set; }
        public DbSet<StressData> StressData { get; set; }
        public DbSet<LiveData> LiveData { get; set; }
    }
}
