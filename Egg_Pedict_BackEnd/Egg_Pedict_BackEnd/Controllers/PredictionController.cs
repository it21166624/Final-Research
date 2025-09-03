using Egg_Pedict_BackEnd.Data;
using Egg_Pedict_BackEnd.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Egg_Pedict_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("EggPredict")]
        public async Task<IActionResult> Predict([FromBody] SenserDataNew data)
        {
            if (data == null)
            {
                return BadRequest("Invalid input data");
            }
            _context.senserDataNew.Add(data);
            //Prev_data prev_Data = new Prev_data() {
            //Egg_count = data.Egg_count,
            //    Hen_Age_weeks = data.Hen_Age_weeks,
            //    Feed_Quantity = data.Feed_Quantity,
            //    Health_Status = data.Health_Status,
            //    Hen_Count = data.Hen_Count,
            //    Humidity = data.Humidity,
            //    Light_Hours = data.Light_Hours,
            //    Temperature = data.Temperature

            //};
            //_context.Prev_data.Add(prev_Data);
            await _context.SaveChangesAsync();

            using var client = new HttpClient();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5000/predict", content);
            var result = await response.Content.ReadAsStringAsync();

            return Ok(result);
        }

        [HttpPost("AnimalStress")]
        public async Task<IActionResult> GetAnimalStress([FromBody] StressData data)
        {
            var client = new HttpClient();
            var response = await client.PostAsJsonAsync("http://localhost:5000/predict_stress", data);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        //[HttpPost("lightDecision")]
        //public async Task<IActionResult> GetlightDecision([FromBody] LightData data)
        //{
        //    var client = new HttpClient();
        //    var response = await client.PostAsJsonAsync("http://localhost:5000/light-decision", data);
        //    var result = await response.Content.ReadAsStringAsync();
        //    return Ok(result);
        //}

    }
}
