using SatellitePipeline.Domain;

namespace SatellitePipeline.Application;

public interface ICommand
{
    Guid CommandId { get; }
    Guid CorrelationId { get; }
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken ct);
}

public interface ICommandBus
{
    Task Send<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand;
}

public interface IAppDb
{
    List<SatelliteProcessingRun> Runs { get; }
    List<SatelliteArtifact> Artifacts { get; }
    List<OutboxMessage> OutboxMessages { get; }
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IOutbox
{
    void Add<TMessage>(TMessage message)
        where TMessage : notnull;
}

public interface IClock
{
    DateTime UtcNow { get; }
}
