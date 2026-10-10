using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.DTOs.SensorReading;
using Shared.DTOs.Setting;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController(SettingsService settingsService) : ControllerBase
    {
        [HttpPut]
        public async Task<ActionResult> EditSetting([FromBody] SettingDto dto) // from web app
        {
            await settingsService.EditAsync(dto);
            return Ok();
        }

        [HttpGet("unapplied")]
        public async Task<ActionResult<List<SettingDto>>> GetAllUnapplied() // from esp
        {
            var data = await settingsService.GetAllUnappliedAsync();
            return Ok(data);
        }

        [HttpGet]
        public async Task<ActionResult<List<SettingDto>>> GetAll() // from esp
        {
            var data = await settingsService.GetAllAsync();
            return Ok(data);
        }

        [HttpPost("apply")]
        public async Task<ActionResult<List<SettingDto>>> ApplySettings([FromBody] List<SettingDto> settings) // from esp
        {
            await settingsService.ApplySettingsAsync(settings);
            return Ok();  
        }
    }
}
