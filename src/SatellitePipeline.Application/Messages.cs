namespace SatellitePipeline.Application;

public sealed record SatelliteProcessingRequested(
    Guid CorrelationId,
    Guid RunId,
    Guid FieldId,
    int GeometryVersion,
    DateOnly PeriodFrom,
    DateOnly PeriodTo);

public sealed record CreateIndexRequested(
    Guid CorrelationId,
    Guid RunId,
    Guid FieldId,
    int GeometryVersion,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    string IndexType);

public sealed record IndexReady(
    Guid CorrelationId,
    Guid RunId,
    string IndexType,
    string MinioKey);

public sealed record CreateTiffRequested(
    Guid CorrelationId,
    Guid RunId,
    Guid FieldId,
    int GeometryVersion,
    DateOnly PeriodFrom,
    DateOnly PeriodTo);

public sealed record TiffReady(
    Guid CorrelationId,
    Guid RunId,
    string MinioKey);

public sealed record CreatePreviewRequested(
    Guid CorrelationId,
    Guid RunId,
    Guid FieldId,
    int GeometryVersion,
    DateOnly PeriodFrom,
    DateOnly PeriodTo);

public sealed record PreviewReady(
    Guid CorrelationId,
    Guid RunId,
    string MinioKey);

public sealed record SatelliteProcessingCompleted(
    Guid CorrelationId,
    Guid RunId);
