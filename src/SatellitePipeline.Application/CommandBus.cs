namespace SatellitePipeline.Application;

public sealed class CommandBus : ICommandBus
{
    private readonly Dictionary<Type, Func<ICommand, CancellationToken, Task>> handlers = new();

    public void Register<TCommand>(ICommandHandler<TCommand> handler)
        where TCommand : ICommand
    {
        handlers[typeof(TCommand)] = (command, ct) => handler.Handle((TCommand)command, ct);
    }

    public Task Send<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        if (!handlers.TryGetValue(typeof(TCommand), out var handler))
            throw new InvalidOperationException($"No command handler registered for {typeof(TCommand).Name}");

        return handler(command, ct);
    }
}
