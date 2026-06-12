using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure;

public sealed class SatelliteBackfillJob
{
    private readonly ICommandBus commandBus;

    public SatelliteBackfillJob(ICommandBus commandBus)
    {
        this.commandBus = commandBus;
    }

    public Task Execute(CancellationToken ct)
    {
        return commandBus.Send(new StartSatelliteProcessingCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            3,
            "brazil",
            "soy",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "quartz:nightly-backfill"), ct);
    }
}
