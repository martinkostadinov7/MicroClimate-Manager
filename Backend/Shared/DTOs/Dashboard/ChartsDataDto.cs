using Shared.DTOs.Dashboard.Statistics;

namespace Shared.DTOs.Dashboard
{
    public class ChartsDataDto
    {
        public double TemperatureLowest { get; set; }
        public double TemperatureHighest { get; set; }
        public double TemperatureAverage { get; set; }
        public double HumidityLowest { get; set; }
        public double HumidityHighest { get; set; }
        public double HumidityAverage { get; set; }
        public double MoistureLowest { get; set; }
        public double MoistureHighest { get; set; }
        public double MoistureAverage { get; set; }
    }
}
