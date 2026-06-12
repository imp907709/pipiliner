using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure;

public sealed class PipelineComposition
{
    public IAppDb Db { get; }
    public CommandBus CommandBus { get; }
    public InMemoryRabbitMqBus RabbitMqBus { get; }
    public OutboxDispatcher OutboxDispatcher { get; }
    public SatelliteBackfillJob BackfillJob { get; }

    private PipelineComposition(
        IAppDb db,
        CommandBus commandBus,
        InMemoryRabbitMqBus rabbitMqBus,
        OutboxDispatcher outboxDispatcher,
        SatelliteBackfillJob backfillJob)
    {
        Db = db;
        CommandBus = commandBus;
        RabbitMqBus = rabbitMqBus;
        OutboxDispatcher = outboxDispatcher;
        BackfillJob = backfillJob;
    }

    public static PipelineComposition Create()
    {
        IAppDb db = new InMemoryAppDb();
        IClock clock = new SystemClock();
        IOutbox outbox = new Outbox(db, clock);
        var commandBus = new CommandBus();
        var rabbitMqBus = new InMemoryRabbitMqBus();

        RegisterCommandHandlers(commandBus, db, outbox, clock);
        RegisterConsumers(rabbitMqBus, commandBus, db);

        var dispatcher = new OutboxDispatcher(db, rabbitMqBus, clock);
        var backfillJob = new SatelliteBackfillJob(commandBus);

        return new PipelineComposition(db, commandBus, rabbitMqBus, dispatcher, backfillJob);
    }

    private static void RegisterCommandHandlers(
        CommandBus commandBus,
        IAppDb db,
        IOutbox outbox,
        IClock clock)
    {
        commandBus.Register(new StartSatelliteProcessingHandler(db, outbox, clock));
        commandBus.Register(new RequestIndexHandler(db, outbox, clock));
        commandBus.Register(new CompleteIndexHandler(db, outbox, clock));
        commandBus.Register(new RequestTiffHandler(db, outbox, clock));
        commandBus.Register(new CompleteTiffHandler(db, outbox, clock));
        commandBus.Register(new RequestPreviewHandler(db, outbox, clock));
        commandBus.Register(new CompletePreviewHandler(db, outbox, clock));
        commandBus.Register(new CompleteSatelliteProcessingHandler(db, outbox, clock));
    }

    private static void RegisterConsumers(
        InMemoryRabbitMqBus rabbitMqBus,
        ICommandBus commandBus,
        IAppDb db)
    {
        var orchestrator = new SatelliteProcessOrchestrator(commandBus);
        var indexWorker = new IndexWorker(commandBus, db, new FakeIndexProcessor());
        var tiffWorker = new TiffWorker(commandBus, db, new FakeTiffProcessor());
        var previewWorker = new PreviewWorker(commandBus, db, new FakePreviewProcessor());

        rabbitMqBus.Subscribe<SatelliteProcessingRequested>(orchestrator.Handle);
        rabbitMqBus.Subscribe<IndexReady>(orchestrator.Handle);
        rabbitMqBus.Subscribe<TiffReady>(orchestrator.Handle);
        rabbitMqBus.Subscribe<PreviewReady>(orchestrator.Handle);

        rabbitMqBus.Subscribe<CreateIndexRequested>(indexWorker.Handle);
        rabbitMqBus.Subscribe<CreateTiffRequested>(tiffWorker.Handle);
        rabbitMqBus.Subscribe<CreatePreviewRequested>(previewWorker.Handle);
    }
}
