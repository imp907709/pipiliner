namespace SatellitePipeline.Domain;

public static class ProcessingStatuses
{
    public const string Pending = "pending";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
}

public static class ProcessingSteps
{
    public const string Start = "start";
    public const string CreateIndex = "create-index";
    public const string CreateTiff = "create-tiff";
    public const string CreatePreview = "create-preview";
    public const string Completed = "completed";
}

public static class ArtifactTypes
{
    public const string Index = "index";
    public const string Tiff = "tiff";
    public const string Preview = "preview";
}

public static class ArtifactStatuses
{
    public const string Ready = "ready";
}
