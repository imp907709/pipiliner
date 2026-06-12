namespace SatellitePipeline.Domain;

public sealed class SatelliteArtifact
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid RunId { get; init; }
    public Guid FieldId { get; init; }
    public int GeometryVersion { get; init; }
    public string ArtifactType { get; init; } = string.Empty;
    public string IndexType { get; init; } = "none";
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public string MinioKey { get; init; } = string.Empty;
    public string Checksum { get; init; } = string.Empty;
    public string Status { get; init; } = ArtifactStatuses.Ready;
    public DateTime CreatedAt { get; init; }
}
