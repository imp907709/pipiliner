using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task Publish(OutboxMessage outboxMessage, CancellationToken ct);
}
