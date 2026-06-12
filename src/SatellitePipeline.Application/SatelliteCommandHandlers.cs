using SatellitePipeline.Domain;

namespace SatellitePipeline.Application;

public sealed class StartSatelliteProcessingHandler : ICommandHandler<StartSatelliteProcessingCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public StartSatelliteProcessingHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(StartSatelliteProcessingCommand command, CancellationToken ct)
    {
        var existingRun = db.Runs.FirstOrDefault(x =>
            x.FieldId == command.FieldId &&
            x.GeometryVersion == command.GeometryVersion &&
            x.PeriodFrom == command.PeriodFrom &&
            x.PeriodTo == command.PeriodTo);

        if (existingRun is not null)
            return;

        var run = SatelliteProcessingRun.Start(
            command.FieldId,
            command.GeometryVersion,
            command.Country,
            command.Crop,
            command.PeriodFrom,
            command.PeriodTo,
            command.RequestedBy,
            clock.UtcNow);

        db.Add(run);

        outbox.Add(new SatelliteProcessingRequested(
            command.CorrelationId,
            run.Id,
            run.FieldId,
            run.GeometryVersion,
            run.PeriodFrom,
            run.PeriodTo));

        await db.SaveChangesAsync(ct);
    }
}

public sealed class RequestIndexHandler : ICommandHandler<RequestIndexCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public RequestIndexHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(RequestIndexCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        run.MoveTo(ProcessingSteps.CreateIndex, clock.UtcNow);

        outbox.Add(new CreateIndexRequested(
            command.CorrelationId,
            run.Id,
            run.FieldId,
            run.GeometryVersion,
            run.PeriodFrom,
            run.PeriodTo,
            command.IndexType));

        await db.SaveChangesAsync(ct);
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class CompleteIndexHandler : ICommandHandler<CompleteIndexCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public CompleteIndexHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(CompleteIndexCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        AddArtifactIfMissing(run, ArtifactTypes.Index, command.IndexType, command.MinioKey, command.Checksum);

        outbox.Add(new IndexReady(
            command.CorrelationId,
            run.Id,
            command.IndexType,
            command.MinioKey));

        await db.SaveChangesAsync(ct);
    }

    private void AddArtifactIfMissing(
        SatelliteProcessingRun run,
        string artifactType,
        string indexType,
        string minioKey,
        string checksum)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == run.Id &&
            x.ArtifactType == artifactType &&
            x.IndexType == indexType);

        if (exists)
            return;

        db.Add(new SatelliteArtifact
        {
            RunId = run.Id,
            FieldId = run.FieldId,
            GeometryVersion = run.GeometryVersion,
            ArtifactType = artifactType,
            IndexType = indexType,
            PeriodFrom = run.PeriodFrom,
            PeriodTo = run.PeriodTo,
            MinioKey = minioKey,
            Checksum = checksum,
            CreatedAt = clock.UtcNow
        });
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class RequestTiffHandler : ICommandHandler<RequestTiffCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public RequestTiffHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(RequestTiffCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        run.MoveTo(ProcessingSteps.CreateTiff, clock.UtcNow);

        outbox.Add(new CreateTiffRequested(
            command.CorrelationId,
            run.Id,
            run.FieldId,
            run.GeometryVersion,
            run.PeriodFrom,
            run.PeriodTo));

        await db.SaveChangesAsync(ct);
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class CompleteTiffHandler : ICommandHandler<CompleteTiffCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public CompleteTiffHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(CompleteTiffCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        AddArtifactIfMissing(run, ArtifactTypes.Tiff, "none", command.MinioKey, command.Checksum);

        outbox.Add(new TiffReady(command.CorrelationId, run.Id, command.MinioKey));

        await db.SaveChangesAsync(ct);
    }

    private void AddArtifactIfMissing(
        SatelliteProcessingRun run,
        string artifactType,
        string indexType,
        string minioKey,
        string checksum)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == run.Id &&
            x.ArtifactType == artifactType &&
            x.IndexType == indexType);

        if (exists)
            return;

        db.Add(new SatelliteArtifact
        {
            RunId = run.Id,
            FieldId = run.FieldId,
            GeometryVersion = run.GeometryVersion,
            ArtifactType = artifactType,
            IndexType = indexType,
            PeriodFrom = run.PeriodFrom,
            PeriodTo = run.PeriodTo,
            MinioKey = minioKey,
            Checksum = checksum,
            CreatedAt = clock.UtcNow
        });
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class RequestPreviewHandler : ICommandHandler<RequestPreviewCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public RequestPreviewHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(RequestPreviewCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        run.MoveTo(ProcessingSteps.CreatePreview, clock.UtcNow);

        outbox.Add(new CreatePreviewRequested(
            command.CorrelationId,
            run.Id,
            run.FieldId,
            run.GeometryVersion,
            run.PeriodFrom,
            run.PeriodTo));

        await db.SaveChangesAsync(ct);
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class CompletePreviewHandler : ICommandHandler<CompletePreviewCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public CompletePreviewHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(CompletePreviewCommand command, CancellationToken ct)
    {
        var run = FindRun(command.RunId);
        AddArtifactIfMissing(run, ArtifactTypes.Preview, "none", command.MinioKey, command.Checksum);

        outbox.Add(new PreviewReady(command.CorrelationId, run.Id, command.MinioKey));

        await db.SaveChangesAsync(ct);
    }

    private void AddArtifactIfMissing(
        SatelliteProcessingRun run,
        string artifactType,
        string indexType,
        string minioKey,
        string checksum)
    {
        var exists = db.Artifacts.Any(x =>
            x.RunId == run.Id &&
            x.ArtifactType == artifactType &&
            x.IndexType == indexType);

        if (exists)
            return;

        db.Add(new SatelliteArtifact
        {
            RunId = run.Id,
            FieldId = run.FieldId,
            GeometryVersion = run.GeometryVersion,
            ArtifactType = artifactType,
            IndexType = indexType,
            PeriodFrom = run.PeriodFrom,
            PeriodTo = run.PeriodTo,
            MinioKey = minioKey,
            Checksum = checksum,
            CreatedAt = clock.UtcNow
        });
    }

    private SatelliteProcessingRun FindRun(Guid runId) =>
        db.Runs.FirstOrDefault(x => x.Id == runId)
        ?? throw new InvalidOperationException($"Run not found: {runId}");
}

public sealed class CompleteSatelliteProcessingHandler : ICommandHandler<CompleteSatelliteProcessingCommand>
{
    private readonly IAppDb db;
    private readonly IOutbox outbox;
    private readonly IClock clock;

    public CompleteSatelliteProcessingHandler(IAppDb db, IOutbox outbox, IClock clock)
    {
        this.db = db;
        this.outbox = outbox;
        this.clock = clock;
    }

    public async Task Handle(CompleteSatelliteProcessingCommand command, CancellationToken ct)
    {
        var run = db.Runs.FirstOrDefault(x => x.Id == command.RunId)
            ?? throw new InvalidOperationException($"Run not found: {command.RunId}");

        run.Complete(clock.UtcNow);
        outbox.Add(new SatelliteProcessingCompleted(command.CorrelationId, run.Id));

        await db.SaveChangesAsync(ct);
    }
}
