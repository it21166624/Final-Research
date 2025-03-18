using Egg_Pedict_BackEnd.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Egg_Pedict_BackEnd.Controllers
{
    [Route("api/sensordata")]
    [ApiController]
    public class SenserDataController : Controller
    {
        private readonly AppDbContext _context;
        public SenserDataController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("lighthours")]
        public async Task<ActionResult<IEnumerable<object>>> GetLightHours()
        {
            var ldrReadings = await _context.LDRData
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

            return Ok(lightHours);
        }

        [HttpGet("liveData")]
        public async Task<ActionResult<IEnumerable<object>>> GetLiveData()
        {
            var liveData = await _context.LiveData
                .ToListAsync();

            return Ok(liveData);
        }
    }
}
