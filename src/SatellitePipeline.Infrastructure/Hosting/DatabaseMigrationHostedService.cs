using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SatellitePipeline.Infrastructure.Persistence;

namespace SatellitePipeline.Infrastructure.Hosting;

public sealed class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly InfrastructureOptions options;
    private readonly ILogger<DatabaseMigrationHostedService> logger;

    public DatabaseMigrationHostedService(
        IServiceScopeFactory scopeFactory,
        InfrastructureOptions options,
        ILogger<DatabaseMigrationHostedService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.options = options;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.UsesPostgres)
            return;

        for (var attempt = 1; attempt <= 30; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                logger.LogInformation("Applying EF Core migrations (attempt {Attempt})", attempt);
                await db.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < 30 && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Database migration attempt {Attempt} failed; retrying", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
