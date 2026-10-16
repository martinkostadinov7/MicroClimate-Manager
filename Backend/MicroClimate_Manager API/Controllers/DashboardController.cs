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
        
        [HttpGet("statistics")]
        public async Task<ActionResult<StatisticsDto>> GetStatistics([FromQuery] QueryParamsStatisticsDto queryParamsStatisticsDto)
        {
            var data = await dashboardService.GetStatisticsAsync(queryParamsStatisticsDto);
            return Ok(data);
        }
    }
}
