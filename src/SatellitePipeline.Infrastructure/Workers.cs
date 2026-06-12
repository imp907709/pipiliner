using SatellitePipeline.Application;
using SatellitePipeline.Domain;

namespace SatellitePipeline.Infrastructure;

public sealed class IndexWorker
{
    private readonly ICommandBus commandBus;
    private readonly IAppDb db;
    private readonly IIndexProcessor processor;

    public IndexWorker(ICommandBus commandBus, IAppDb db, IIndexProcessor processor)
    {
        this.commandBus = commandBus;
        this.db = db;
        this.processor = processor;
    }

    public async Task Handle(CreateIndexRequested command, CancellationToken ct)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == command.RunId &&
            x.ArtifactType == ArtifactTypes.Index &&
            x.IndexType == command.IndexType);

        if (exists)
            return;

        var result = await processor.CreateNdvi(command, ct);

        await commandBus.Send(new CompleteIndexCommand(
            Guid.NewGuid(),
            command.CorrelationId,
            command.RunId,
            command.IndexType,
            result.MinioKey,
            result.Checksum), ct);
    }
}

public sealed class TiffWorker
{
    private readonly ICommandBus commandBus;
    private readonly IAppDb db;
    private readonly ITiffProcessor processor;

    public TiffWorker(ICommandBus commandBus, IAppDb db, ITiffProcessor processor)
    {
        this.commandBus = commandBus;
        this.db = db;
        this.processor = processor;
    }

    public async Task Handle(CreateTiffRequested command, CancellationToken ct)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == command.RunId &&
            x.ArtifactType == ArtifactTypes.Tiff);

        if (exists)
            return;

        var result = await processor.CreateTiff(command, ct);

        await commandBus.Send(new CompleteTiffCommand(
            Guid.NewGuid(),
            command.CorrelationId,
            command.RunId,
            result.MinioKey,
            result.Checksum), ct);
    }
}

public sealed class PreviewWorker
{
    private readonly ICommandBus commandBus;
    private readonly IAppDb db;
    private readonly IPreviewProcessor processor;

    public PreviewWorker(ICommandBus commandBus, IAppDb db, IPreviewProcessor processor)
    {
        this.commandBus = commandBus;
        this.db = db;
        this.processor = processor;
    }

    public async Task Handle(CreatePreviewRequested command, CancellationToken ct)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == command.RunId &&
            x.ArtifactType == ArtifactTypes.Preview);

        if (exists)
            return;

        var result = await processor.CreatePreview(command, ct);

        await commandBus.Send(new CompletePreviewCommand(
            Guid.NewGuid(),
            command.CorrelationId,
            command.RunId,
            result.MinioKey,
            result.Checksum), ct);
    }
}
