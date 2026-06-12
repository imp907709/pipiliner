using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SatellitePipeline.Application;
using SatellitePipeline.Infrastructure.Hosting;
using SatellitePipeline.Infrastructure.Messaging;
using SatellitePipeline.Infrastructure.Persistence;

namespace SatellitePipeline.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static InfrastructureOptions AddSatellitePipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection("Infrastructure").Get<InfrastructureOptions>()
            ?? new InfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<MessageDispatcher>(RegisterMessageConsumers);

        if (options.UsesPostgres)
        {
            services.AddDbContext<AppDbContext>(builder =>
                builder.UseNpgsql(options.PostgresConnectionString));
            services.AddScoped<IAppDb, EfAppDb>();
            services.AddHostedService<DatabaseMigrationHostedService>();
        }
        else
        {
            services.AddSingleton<IAppDb, InMemoryAppDb>();
        }

        if (options.UsesRabbitMq)
            services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
        else
            services.AddSingleton<IMessagePublisher, InMemoryMessagePublisher>();

        services.AddScoped<IIndexProcessor, FakeIndexProcessor>();
        services.AddScoped<ITiffProcessor, FakeTiffProcessor>();
        services.AddScoped<IPreviewProcessor, FakePreviewProcessor>();
        services.AddScoped<SatelliteProcessOrchestrator>();
        services.AddScoped<IndexWorker>();
        services.AddScoped<TiffWorker>();
        services.AddScoped<PreviewWorker>();
        services.AddScoped<PipelineComposition>();
        services.AddScoped<ICommandBus>(sp => sp.GetRequiredService<PipelineComposition>().CommandBus);

        return options;
    }

    public static void AddSatellitePipelineWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        bool runBackfillOnStartup = false)
    {
        var options = services.AddSatellitePipeline(configuration);

        services.AddHostedService<OutboxDispatcherHostedService>();

        if (options.UsesRabbitMq)
            services.AddHostedService<RabbitMqConsumerHostedService>();

        if (runBackfillOnStartup)
            services.AddHostedService<BackfillOnStartupHostedService>();
    }

    private static MessageDispatcher RegisterMessageConsumers(IServiceProvider serviceProvider)
    {
        var dispatcher = new MessageDispatcher();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        dispatcher.Subscribe<SatelliteProcessingRequested>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SatelliteProcessOrchestrator>().Handle(message, ct);
        });

        dispatcher.Subscribe<IndexReady>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SatelliteProcessOrchestrator>().Handle(message, ct);
        });

        dispatcher.Subscribe<TiffReady>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SatelliteProcessOrchestrator>().Handle(message, ct);
        });

        dispatcher.Subscribe<PreviewReady>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<SatelliteProcessOrchestrator>().Handle(message, ct);
        });

        dispatcher.Subscribe<CreateIndexRequested>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IndexWorker>().Handle(message, ct);
        });

        dispatcher.Subscribe<CreateTiffRequested>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<TiffWorker>().Handle(message, ct);
        });

        dispatcher.Subscribe<CreatePreviewRequested>(async (message, ct) =>
        {
            using var scope = scopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<PreviewWorker>().Handle(message, ct);
        });

        return dispatcher;
    }
}
