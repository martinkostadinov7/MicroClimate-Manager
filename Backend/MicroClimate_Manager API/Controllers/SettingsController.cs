using Microsoft.AspNetCore.Mvc;
using Services;
using Shared.DTOs.Setting;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController(SettingsService settingsService) : ControllerBase
    {
        [HttpPut]
        public async Task<ActionResult> EditSettings([FromBody] List<SettingDto> dtos) // from web app
        {
            await settingsService.EditSettingsAsync(dtos);
            return Ok();
        }

        [HttpGet("unapplied")]
        public async Task<ActionResult<List<SettingDto>>> GetAllUnapplied() // from esp
        {
            var data = await settingsService.GetAllUnappliedAsync();
            return Ok(data);
        }

        [HttpGet]
        public async Task<ActionResult<List<SettingDto>>> GetAll() // from esp and web app
        {
            var data = await settingsService.GetAllAsync();
            return Ok(data);
        }

        [HttpPost("apply")]
        public async Task<ActionResult<List<SettingDto>>> ApplySettings([FromBody] List<SettingDto> settings) // from esp
        {
            await settingsService.ApplySettingsAsync(settings);
            return NoContent();
        }
    }
}
