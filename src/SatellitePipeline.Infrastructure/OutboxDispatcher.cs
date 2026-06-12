using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure;

public sealed class OutboxDispatcher
{
    private readonly IAppDb db;
    private readonly InMemoryRabbitMqBus rabbitMqBus;
    private readonly IClock clock;

    public OutboxDispatcher(IAppDb db, InMemoryRabbitMqBus rabbitMqBus, IClock clock)
    {
        this.db = db;
        this.rabbitMqBus = rabbitMqBus;
        this.clock = clock;
    }

    public async Task<int> DrainOnce(CancellationToken ct)
    {
        var pending = db.OutboxMessages
            .Where(x => x.Status == OutboxStatuses.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(100)
            .ToList();

        foreach (var message in pending)
        {
            try
            {
                await rabbitMqBus.Publish(message, ct);
                message.MarkPublished(clock.UtcNow);
            }
            catch (Exception ex)
            {
                message.MarkFailed(ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
        return pending.Count;
    }
}
