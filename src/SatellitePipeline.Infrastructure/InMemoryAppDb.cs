using SatellitePipeline.Application;
using SatellitePipeline.Domain;

namespace SatellitePipeline.Infrastructure;

public sealed class InMemoryAppDb : IAppDb
{
    public List<SatelliteProcessingRun> Runs { get; } = [];
    public List<SatelliteArtifact> Artifacts { get; } = [];
    public List<OutboxMessage> OutboxMessages { get; } = [];

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
        db.OutboxMessages.Add(new OutboxMessage
        {
            Type = MessageSerializer.GetMessageType<TMessage>(),
            PayloadJson = MessageSerializer.Serialize(message),
            CreatedAt = clock.UtcNow
        });
    }
}
