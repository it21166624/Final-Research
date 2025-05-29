using System.ComponentModel.DataAnnotations;

namespace Egg_Pedict_BackEnd.Model
{
    public class StressData
    {
        [Key]
        public int Id { get; set; }
        public double Temperature { get; set; }  // Environmental Temperature
        public double Humidity { get; set; }
        public double FeedIntakePerHen { get; set; }
        public double WaterIntakePerHen { get; set; }
        public string AirQuality { get; set; }
        public double Lighting { get; set; }
        public double CageDensity { get; set; }
        public double Vocalization { get; set; }
        public double BodyTemperature { get; set; }  // New Field
        public int Heartbeat { get; set; }  // New Field
        public int StressLevel { get; set; }  // 0 = Low, 1 = Medium, 2 = High
        public DateTime Timestamp { get; set; }
    }
}
