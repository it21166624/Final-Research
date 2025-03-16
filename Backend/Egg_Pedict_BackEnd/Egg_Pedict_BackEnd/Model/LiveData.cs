using System.ComponentModel.DataAnnotations;

namespace Egg_Pedict_BackEnd.Model
{
    public class LiveData
    {
        [Key]
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Light_Hours { get; set; }
        public double Hen_Age_weeks { get; set; }
        public double Feed_Quantity { get; set; }
        public double Hen_Count { get; set; }
        public double Egg_count { get; set; }
        public string Health_Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
