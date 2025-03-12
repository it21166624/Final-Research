using Egg_Pedict_BackEnd.Model;
using Microsoft.EntityFrameworkCore;

namespace Egg_Pedict_BackEnd.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<SensorData> SensorData { get; set; }
    }
}
