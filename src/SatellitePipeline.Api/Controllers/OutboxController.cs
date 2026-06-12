using Microsoft.AspNetCore.Mvc;
using SatellitePipeline.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace SatellitePipeline.Api.Controllers
{
    [ApiController]
    [Route("outbox")]
    public class OutboxController : ControllerBase
    {
        private readonly PipelineComposition pipeline;

        public OutboxController(PipelineComposition pipeline)
        {
            this.pipeline = pipeline;
        }

        [HttpPost("drain")]
        public async Task<IActionResult> Drain(CancellationToken ct)
        {
            var dispatched = await pipeline.OutboxDispatcher.DrainOnce(ct);
            return Ok(new { dispatched });
        }
    }
}
