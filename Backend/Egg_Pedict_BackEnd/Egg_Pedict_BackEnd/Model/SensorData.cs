namespace Egg_Pedict_BackEnd.Model
{
    public class SensorData
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
