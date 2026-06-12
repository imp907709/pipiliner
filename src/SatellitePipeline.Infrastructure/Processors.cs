using SatellitePipeline.Application;

namespace SatellitePipeline.Infrastructure;

public interface IIndexProcessor
{
    Task<ArtifactResult> CreateNdvi(CreateIndexRequested command, CancellationToken ct);
}

public interface ITiffProcessor
{
    Task<ArtifactResult> CreateTiff(CreateTiffRequested command, CancellationToken ct);
}

public interface IPreviewProcessor
{
    Task<ArtifactResult> CreatePreview(CreatePreviewRequested command, CancellationToken ct);
}

public sealed record ArtifactResult(string MinioKey, string Checksum);

public sealed class FakeIndexProcessor : IIndexProcessor
{
    public Task<ArtifactResult> CreateNdvi(CreateIndexRequested command, CancellationToken ct)
    {
        var key = $"satellite/{command.FieldId}/g{command.GeometryVersion}/index/{command.IndexType}/{command.PeriodFrom:yyyyMMdd}-{command.PeriodTo:yyyyMMdd}.tif";
        return Task.FromResult(new ArtifactResult(key, $"checksum-index-{command.RunId:N}"));
    }
}

public sealed class FakeTiffProcessor : ITiffProcessor
{
    public Task<ArtifactResult> CreateTiff(CreateTiffRequested command, CancellationToken ct)
    {
        var key = $"satellite/{command.FieldId}/g{command.GeometryVersion}/tiff/{command.PeriodFrom:yyyyMMdd}-{command.PeriodTo:yyyyMMdd}.tif";
        return Task.FromResult(new ArtifactResult(key, $"checksum-tiff-{command.RunId:N}"));
    }
}

public sealed class FakePreviewProcessor : IPreviewProcessor
{
    public Task<ArtifactResult> CreatePreview(CreatePreviewRequested command, CancellationToken ct)
    {
        var key = $"satellite/{command.FieldId}/g{command.GeometryVersion}/preview/{command.PeriodFrom:yyyyMMdd}-{command.PeriodTo:yyyyMMdd}.webp";
        return Task.FromResult(new ArtifactResult(key, $"checksum-preview-{command.RunId:N}"));
    }
}
