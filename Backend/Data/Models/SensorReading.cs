namespace Data.Models
{
    public class SensorReading
    {
        public int Id { get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        public double SoilMoisture { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
