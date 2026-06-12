using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure.Messaging;

public sealed class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly MessageDispatcher dispatcher;

    public InMemoryMessagePublisher(MessageDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
    }

    public Task Publish(OutboxMessage outboxMessage, CancellationToken ct) =>
        dispatcher.DispatchAsync(outboxMessage.Type, outboxMessage.PayloadJson, ct);
}
