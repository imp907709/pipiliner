using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure.Messaging;

public sealed class MessageDispatcher
{
    private readonly Dictionary<string, List<Func<object, CancellationToken, Task>>> handlers = new();

    public void Subscribe<TMessage>(Func<TMessage, CancellationToken, Task> handler)
        where TMessage : notnull
    {
        var typeName = typeof(TMessage).Name;

        if (!handlers.TryGetValue(typeName, out var messageHandlers))
        {
            messageHandlers = [];
            handlers[typeName] = messageHandlers;
        }

        messageHandlers.Add((message, ct) => handler((TMessage)message, ct));
    }

    public async Task DispatchAsync(string type, string payloadJson, CancellationToken ct)
    {
        var message = MessageSerializer.Deserialize(type, payloadJson);

        if (!handlers.TryGetValue(type, out var messageHandlers))
            return;

        foreach (var handler in messageHandlers)
            await handler(message, ct);
    }
}
