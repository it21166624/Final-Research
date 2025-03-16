using Egg_Pedict_BackEnd.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Egg_Pedict_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        [HttpPost("EggPredict")]
        public async Task<IActionResult> Predict([FromBody] SensorData data)
        {
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict", data);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        [HttpPost("AnimalStress")]
        public async Task<IActionResult> GetAnimalStress([FromBody] StressData data)
        {
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict", data);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }
    }
}
