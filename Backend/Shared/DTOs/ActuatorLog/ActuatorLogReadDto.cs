namespace Shared.DTOs.ActuatorLog
{
    public class ActuatorLogReadDto
    {
        public string DeviceName { get; set; }

        public int State { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
