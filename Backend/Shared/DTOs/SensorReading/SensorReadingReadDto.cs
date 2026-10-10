namespace Shared.DTOs.SensorReading
{
    public class SensorReadingReadDto
    {
        public double Temperature { get; set; }

        public double Humidity { get; set; }

        public double SoilMoisture { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
