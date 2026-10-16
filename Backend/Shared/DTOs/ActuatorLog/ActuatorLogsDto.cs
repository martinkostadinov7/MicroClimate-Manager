namespace Shared.DTOs.ActuatorLog
{
    public class ActuatorLogsDto
    {
        public ActuatorLogReadDto? Heater { get; set; }
        public ActuatorLogReadDto? Fan { get; set; }
        public ActuatorLogReadDto? Pump { get; set; }
    }
}
