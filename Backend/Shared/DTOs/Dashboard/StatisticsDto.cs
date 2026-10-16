using Shared.DTOs.Dashboard.Statistics;

namespace Shared.DTOs.Dashboard
{
    public class StatisticsDto
    {
        public List<TemperatureStatisticsDto> Temperature { get; set; }
        public List<HumidityStatisticsDto> Humidity { get; set; }
        public List<MoistureStatisticsDto> Moisture { get; set; }

        public List<HeaterStatisticsDto> Heater { get; set; }
        //public List<FanStatisticsDto> Fan { get; set; }
        //public List<PumpStatisticsDto> Pump { get; set; }
    }
}
