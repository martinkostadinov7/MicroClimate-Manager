using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.DTOs.SensorReading;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorReadingsController(SensorReadingsService sensorReadingsService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> AddSensorReading([FromBody] SensorReadingCreateDto dto)
        {
            await sensorReadingsService.AddAsync(dto);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<List<SensorReadingReadDto>>> GetAllSensorReadings()
        {
            var data = await sensorReadingsService.GetAllAsync();
            return Ok(data);
        }
    }
}