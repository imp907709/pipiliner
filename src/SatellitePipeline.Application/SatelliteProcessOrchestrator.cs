namespace SatellitePipeline.Application;

public sealed class SatelliteProcessOrchestrator
{
    private readonly ICommandBus commandBus;

    public SatelliteProcessOrchestrator(ICommandBus commandBus)
    {
        this.commandBus = commandBus;
    }

    public Task Handle(SatelliteProcessingRequested message, CancellationToken ct)
    {
        return commandBus.Send(new RequestIndexCommand(
            Guid.NewGuid(),
            message.CorrelationId,
            message.RunId,
            "ndvi"), ct);
    }

    public Task Handle(IndexReady message, CancellationToken ct)
    {
        return commandBus.Send(new RequestTiffCommand(
            Guid.NewGuid(),
            message.CorrelationId,
            message.RunId), ct);
    }

    public Task Handle(TiffReady message, CancellationToken ct)
    {
        return commandBus.Send(new RequestPreviewCommand(
            Guid.NewGuid(),
            message.CorrelationId,
            message.RunId), ct);
    }

    public Task Handle(PreviewReady message, CancellationToken ct)
    {
        return commandBus.Send(new CompleteSatelliteProcessingCommand(
            Guid.NewGuid(),
            message.CorrelationId,
            message.RunId), ct);
    }
}
