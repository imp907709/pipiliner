namespace SatellitePipeline.Application;

public sealed record StartSatelliteProcessingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid FieldId,
    int GeometryVersion,
    string Country,
    string Crop,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    string RequestedBy) : ICommand;

public sealed record RequestIndexCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId,
    string IndexType) : ICommand;

public sealed record CompleteIndexCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId,
    string IndexType,
    string MinioKey,
    string Checksum) : ICommand;

public sealed record RequestTiffCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId) : ICommand;

public sealed record CompleteTiffCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId,
    string MinioKey,
    string Checksum) : ICommand;

public sealed record RequestPreviewCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId) : ICommand;

public sealed record CompletePreviewCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId,
    string MinioKey,
    string Checksum) : ICommand;

public sealed record CompleteSatelliteProcessingCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid RunId) : ICommand;
