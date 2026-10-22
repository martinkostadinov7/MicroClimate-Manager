using Shared.DTOs.Dashboard.Statistics;

namespace Shared.DTOs.Dashboard
{
    public class ChartsDataDto
    {
        public List<TemperatureRecordDto> Temperature { get; set; }
        public List<HumidityRecordDto> Humidity { get; set; }
        public List<MoistureRecordDto> Moisture { get; set; }

        public List<HeaterStatisticsDto> Heater { get; set; }
        //public List<FanStatisticsDto> Fan { get; set; }
        //public List<PumpStatisticsDto> Pump { get; set; }
    }
}
