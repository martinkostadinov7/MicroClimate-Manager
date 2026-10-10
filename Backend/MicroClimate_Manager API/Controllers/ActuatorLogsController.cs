using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.DTOs.ActuatorLog;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActuatorLogsController(ActuatorLogsService actuatorLogsService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AddActuatorLog([FromBody] ActuatorLogCreateDto dto)
        {
            await actuatorLogsService.AddAsync(dto);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<ActuatorLogReadDto>>> GetAllActuatorLogs()
        {
            var data = await actuatorLogsService.GetAllAsync();
            return Ok(data);
        }
    }
}
