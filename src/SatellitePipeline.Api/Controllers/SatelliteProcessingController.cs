using Microsoft.AspNetCore.Mvc;
using SatellitePipeline.Application;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SatellitePipeline.Api.Controllers
{
    [ApiController]
    [Route("satellite-processing")]
    public class SatelliteProcessingController : ControllerBase
    {
        private readonly ICommandBus commandBus;

        public SatelliteProcessingController(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        [HttpPost]
        public async Task<IActionResult> Start(
            [FromBody] StartSatelliteProcessingRequest request,
            CancellationToken ct)
        {
            await commandBus.Send(new StartSatelliteProcessingCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request.FieldId,
                request.GeometryVersion,
                request.Country,
                request.Crop,
                request.PeriodFrom,
                request.PeriodTo,
                "api"), ct);

            return Accepted();
        }
    }
}
