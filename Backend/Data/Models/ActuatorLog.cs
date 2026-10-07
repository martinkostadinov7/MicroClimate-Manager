namespace Data.Models
{
    public class ActuatorLog
    {
        public int Id { get; set; }

        public string DeviceName { get; set; }

        public int State { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
