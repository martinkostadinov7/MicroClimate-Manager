using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        [HttpPost]
        public IActionResult ReceiveData()
        {
            return Ok();
        }
    }
}