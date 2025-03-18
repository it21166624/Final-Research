using Egg_Pedict_BackEnd.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Egg_Pedict_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        [HttpPost("EggPredict")]
        public async Task<IActionResult> Predict([FromBody] SenserDataNew data)
        {
            if (data == null)
            {
                return BadRequest("Invalid input data");
            }

            using var client = new HttpClient();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5000/predict", content);
            var result = await response.Content.ReadAsStringAsync();

            return Ok(result);
            //var client = new HttpClient();
            //var response = await client.PostAsJsonAsync("http://localhost:5000/predict", data);
            //var result = await response.Content.ReadAsStringAsync();
            //return Ok(result);
        }

        [HttpPost("AnimalStress")]
        public async Task<IActionResult> GetAnimalStress([FromBody] StressData data)
        {
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict_stress", data);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }
    }
}
