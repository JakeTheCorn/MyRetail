using Microsoft.AspNetCore.Mvc;

namespace Api.Features.HealthChecks
{
    [ApiController]
    [Route("[controller]")]
    public class PingController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("pong");
        }
    }
}