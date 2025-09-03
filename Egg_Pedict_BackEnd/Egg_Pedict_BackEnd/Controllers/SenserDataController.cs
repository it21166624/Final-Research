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

        [HttpGet("liveData")]
        public async Task<ActionResult<IEnumerable<object>>> GetLiveData()
        {
            var liveData = await _context.LiveData
                .ToListAsync();

            return Ok(liveData);
        }

        [HttpGet("GetOldData")]
        public async Task<ActionResult<IEnumerable<object>>> GetOldData()
        {
            var oldData = await _context.senserDataNew
                .ToListAsync();

            return Ok(oldData);
        }
    }
}
