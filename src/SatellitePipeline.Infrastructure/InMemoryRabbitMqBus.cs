using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure;

public sealed class InMemoryRabbitMqBus
{
    private readonly Dictionary<Type, List<Func<object, CancellationToken, Task>>> handlers = new();

    public void Subscribe<TMessage>(Func<TMessage, CancellationToken, Task> handler)
        where TMessage : notnull
    {
        if (!handlers.TryGetValue(typeof(TMessage), out var messageHandlers))
        {
            messageHandlers = [];
            handlers[typeof(TMessage)] = messageHandlers;
        }

        messageHandlers.Add((message, ct) => handler((TMessage)message, ct));
    }

    public async Task Publish(OutboxMessage outboxMessage, CancellationToken ct)
    {
        var message = MessageSerializer.Deserialize(outboxMessage.Type, outboxMessage.PayloadJson);

        if (!handlers.TryGetValue(message.GetType(), out var messageHandlers))
            return;

        foreach (var handler in messageHandlers)
            await handler(message, ct);
    }
}
