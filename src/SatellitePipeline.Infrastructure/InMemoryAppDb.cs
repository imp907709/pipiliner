using SatellitePipeline.Application;
using SatellitePipeline.Domain;

namespace SatellitePipeline.Infrastructure;

public sealed class InMemoryAppDb : IAppDb
{
    private readonly List<SatelliteProcessingRun> runs = [];
    private readonly List<SatelliteArtifact> artifacts = [];
    private readonly List<OutboxMessage> outboxMessages = [];

    public IQueryable<SatelliteProcessingRun> Runs => runs.AsQueryable();
    public IQueryable<SatelliteArtifact> Artifacts => artifacts.AsQueryable();
    public IQueryable<OutboxMessage> OutboxMessages => outboxMessages.AsQueryable();

    public void Add(SatelliteProcessingRun run) => runs.Add(run);
    public void Add(SatelliteArtifact artifact) => artifacts.Add(artifact);
    public void Add(OutboxMessage message) => outboxMessages.Add(message);

    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class Outbox : IOutbox
{
    private readonly IAppDb db;
    private readonly IClock clock;

    public Outbox(IAppDb db, IClock clock)
    {
        this.db = db;
        this.clock = clock;
    }

    public void Add<TMessage>(TMessage message)
        where TMessage : notnull
    {
        db.Add(new OutboxMessage
        {
            Type = MessageSerializer.GetMessageType<TMessage>(),
            PayloadJson = MessageSerializer.Serialize(message),
            CreatedAt = clock.UtcNow
        });
    }
}
