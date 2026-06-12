using Microsoft.AspNetCore.Mvc;
using SatellitePipeline.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatellitePipeline.Api.Controllers
{
    [ApiController]
    [Route("runs")]
    public class RunsController : ControllerBase
    {
        private readonly PipelineComposition pipeline;

        public RunsController(PipelineComposition pipeline)
        {
            this.pipeline = pipeline;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                runs = pipeline.Db.Runs.Select(x => new
                {
                    x.Id,
                    x.FieldId,
                    x.GeometryVersion,
                    x.Country,
                    x.Crop,
                    x.PeriodFrom,
                    x.PeriodTo,
                    x.Status,
                    x.CurrentStep
                }),
                artifacts = pipeline.Db.Artifacts.Select(x => new
                {
                    x.RunId,
                    x.ArtifactType,
                    x.IndexType,
                    x.MinioKey,
                    x.Status
                }),
                outbox = pipeline.Db.OutboxMessages.Select(x => new
                {
                    x.Type,
                    x.Status,
                    x.RetryCount,
                    x.LastError
                })
            });
        }
    }
}
