using System.ComponentModel.DataAnnotations;

namespace Egg_Pedict_BackEnd.Model
{
    public class LDRData
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int LDRValue { get; set; }

        public string LightStatus { get; set; } 
    }
}
