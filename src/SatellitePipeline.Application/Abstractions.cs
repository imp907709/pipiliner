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
    IQueryable<SatelliteProcessingRun> Runs { get; }
    IQueryable<SatelliteArtifact> Artifacts { get; }
    IQueryable<OutboxMessage> OutboxMessages { get; }
    void Add(SatelliteProcessingRun run);
    void Add(SatelliteArtifact artifact);
    void Add(OutboxMessage message);
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
