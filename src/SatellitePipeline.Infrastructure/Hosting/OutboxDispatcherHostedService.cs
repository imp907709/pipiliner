using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SatellitePipeline.Infrastructure.Hosting;

public sealed class OutboxDispatcherHostedService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<OutboxDispatcherHostedService> logger;
    private readonly TimeSpan interval;

    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherHostedService> logger)
        : this(scopeFactory, logger, TimeSpan.FromSeconds(2))
    {
    }

    public OutboxDispatcherHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcherHostedService> logger,
        TimeSpan interval)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<PipelineComposition>();
                var dispatched = await pipeline.OutboxDispatcher.DrainOnce(stoppingToken);

                if (dispatched > 0)
                    logger.LogInformation("Outbox drain published {Count} message(s)", dispatched);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Outbox drain failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
