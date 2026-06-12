using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SatellitePipeline.Infrastructure.Hosting;

public sealed class BackfillOnStartupHostedService : BackgroundService
{
    private readonly PipelineComposition pipeline;
    private readonly ILogger<BackfillOnStartupHostedService> logger;

    public BackfillOnStartupHostedService(
        PipelineComposition pipeline,
        ILogger<BackfillOnStartupHostedService> logger)
    {
        this.pipeline = pipeline;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Running satellite backfill on startup");
        await pipeline.BackfillJob.Execute(stoppingToken);
    }
}
