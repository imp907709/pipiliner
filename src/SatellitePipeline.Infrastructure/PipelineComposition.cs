using Microsoft.Extensions.DependencyInjection;
using SatellitePipeline.Application;
using SatellitePipeline.Infrastructure.Messaging;

namespace SatellitePipeline.Infrastructure;

public sealed class PipelineComposition
{
    public IAppDb Db { get; }
    public CommandBus CommandBus { get; }
    public OutboxDispatcher OutboxDispatcher { get; }
    public SatelliteBackfillJob BackfillJob { get; }
    public InfrastructureOptions Options { get; }

    public PipelineComposition(
        IAppDb db,
        IClock clock,
        IMessagePublisher messagePublisher,
        InfrastructureOptions options)
    {
        Db = db;
        Options = options;
        var outbox = new Outbox(db, clock);
        CommandBus = new CommandBus();
        RegisterCommandHandlers(CommandBus, db, outbox, clock);
        OutboxDispatcher = new OutboxDispatcher(db, messagePublisher, clock);
        BackfillJob = new SatelliteBackfillJob(CommandBus);
    }

    public static PipelineComposition Create()
    {
        var db = new InMemoryAppDb();
        var clock = new SystemClock();
        var dispatcher = new MessageDispatcher();
        var publisher = new InMemoryMessagePublisher(dispatcher);
        var options = new InfrastructureOptions();
        var composition = new PipelineComposition(db, clock, publisher, options);
        RegisterInMemoryConsumers(dispatcher, composition);
        return composition;
    }

    private static void RegisterInMemoryConsumers(MessageDispatcher dispatcher, PipelineComposition composition)
    {
        var orchestrator = new SatelliteProcessOrchestrator(composition.CommandBus);
        var indexWorker = new IndexWorker(composition.CommandBus, composition.Db, new FakeIndexProcessor());
        var tiffWorker = new TiffWorker(composition.CommandBus, composition.Db, new FakeTiffProcessor());
        var previewWorker = new PreviewWorker(composition.CommandBus, composition.Db, new FakePreviewProcessor());

        dispatcher.Subscribe<SatelliteProcessingRequested>(orchestrator.Handle);
        dispatcher.Subscribe<IndexReady>(orchestrator.Handle);
        dispatcher.Subscribe<TiffReady>(orchestrator.Handle);
        dispatcher.Subscribe<PreviewReady>(orchestrator.Handle);
        dispatcher.Subscribe<CreateIndexRequested>(indexWorker.Handle);
        dispatcher.Subscribe<CreateTiffRequested>(tiffWorker.Handle);
        dispatcher.Subscribe<CreatePreviewRequested>(previewWorker.Handle);
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
}
