namespace Shared.DTOs.Dashboard.Statistics
{
    public class QueryParamsChartDataDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int PointsCount { get; set; }
    }
}
