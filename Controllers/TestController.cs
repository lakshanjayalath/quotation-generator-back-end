using Microsoft.AspNetCore.Mvc;

namespace quotation_generator_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET: api/Test
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Backend is working!",
                timestamp = DateTime.UtcNow,
                status = "success"
            });
        }

        // GET: api/Test/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "pong",
                server = "Quotation Generator API",
                version = "1.0"
            });
        }
    }
}
