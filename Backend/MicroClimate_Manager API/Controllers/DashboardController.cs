using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.DTOs.Dashboard;
using Shared.DTOs.Dashboard.Statistics;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController(DashboardService dashboardService) : ControllerBase
    {
        [HttpGet("latestData")]
        public async Task<ActionResult<LatestDataDto>> GetLatestData()
        {
            var data = await dashboardService.GetLatestAsync();
            return Ok(data);
        }
        
        [HttpGet("chart-data")]
        public async Task<ActionResult<ChartsDataDto>> GetChartData([FromQuery] QueryParamsChartsDto queryParamsChartsDto)
        {
            var data = await dashboardService.GetChartDataAsync(queryParamsChartsDto);
            return Ok(data);
        }
    }
}
