using Shared.DTOs.Dashboard.Statistics;

namespace Shared.DTOs.Dashboard
{
    public class ChartDataDto
    {
        public List<TemperatureChartDataDto> Temperature { get; set; }
        public List<HumidityChartDataDto> Humidity { get; set; }
        public List<MoistureChartDataDto> Moisture { get; set; }
    }
}
