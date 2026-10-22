namespace Shared.DTOs.Dashboard.Statistics
{
    public class QueryParamsChartsDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int TargetPointsCount { get; set; }
    }
}
