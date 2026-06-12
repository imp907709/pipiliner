using Microsoft.EntityFrameworkCore;
using SatellitePipeline.Application;
using SatellitePipeline.Domain;

namespace SatellitePipeline.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SatelliteProcessingRun> Runs => Set<SatelliteProcessingRun>();
    public DbSet<SatelliteArtifact> Artifacts => Set<SatelliteArtifact>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SatelliteProcessingRun>(entity =>
        {
            entity.ToTable("satellite_processing_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Country).HasMaxLength(64);
            entity.Property(x => x.Crop).HasMaxLength(64);
            entity.Property(x => x.Status).HasMaxLength(32);
            entity.Property(x => x.CurrentStep).HasMaxLength(64);
            entity.Property(x => x.RequestedBy).HasMaxLength(128);
        });

        modelBuilder.Entity<SatelliteArtifact>(entity =>
        {
            entity.ToTable("satellite_artifacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ArtifactType).HasMaxLength(32);
            entity.Property(x => x.IndexType).HasMaxLength(32);
            entity.Property(x => x.MinioKey).HasMaxLength(512);
            entity.Property(x => x.Checksum).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(32);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb");
            entity.Property(x => x.Status).HasMaxLength(32);
            entity.Property(x => x.LastError).HasMaxLength(2048);
        });
    }
}

public sealed class EfAppDb : IAppDb
{
    private readonly AppDbContext context;

    public EfAppDb(AppDbContext context)
    {
        this.context = context;
    }

    public IQueryable<SatelliteProcessingRun> Runs => context.Runs;
    public IQueryable<SatelliteArtifact> Artifacts => context.Artifacts;
    public IQueryable<OutboxMessage> OutboxMessages => context.OutboxMessages;

    public void Add(SatelliteProcessingRun run) => context.Runs.Add(run);
    public void Add(SatelliteArtifact artifact) => context.Artifacts.Add(artifact);
    public void Add(OutboxMessage message) => context.OutboxMessages.Add(message);

    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
