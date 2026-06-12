namespace SatellitePipeline.Application;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public string Status { get; private set; } = OutboxStatuses.Pending;
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    public void MarkPublished(DateTime now)
    {
        Status = OutboxStatuses.Published;
        PublishedAt = now;
        LastError = null;
    }

    public void MarkFailed(string error)
    {
        Status = OutboxStatuses.Failed;
        RetryCount++;
        LastError = error;
    }
}

public static class OutboxStatuses
{
    public const string Pending = "pending";
    public const string Published = "published";
    public const string Failed = "failed";
}
