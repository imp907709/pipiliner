using Microsoft.AspNetCore.Mvc;
using SatellitePipeline.Infrastructure;

namespace SatellitePipeline.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly InfrastructureOptions options;

        public HealthController(InfrastructureOptions options)
        {
            this.options = options;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                postgres = options.UsesPostgres,
                rabbitMq = options.UsesRabbitMq
            });
        }
    }
}
