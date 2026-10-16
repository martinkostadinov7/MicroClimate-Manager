using Shared.DTOs.ActuatorLog;
using Shared.DTOs.SensorReading;

namespace Shared.DTOs.Dashboard
{
    public class LatestDataDto
    {
        public ActuatorLogsDto? LatestActuatorLogs { get; set; }
        public SensorReadingReadDto? LatestSensorReading { get; set; }
    }
}
