namespace SatellitePipeline.Domain;

public sealed class SatelliteProcessingRun
{
    public Guid Id { get; private set; }
    public Guid FieldId { get; private set; }
    public int GeometryVersion { get; private set; }
    public string Country { get; private set; } = string.Empty;
    public string Crop { get; private set; } = string.Empty;
    public DateOnly PeriodFrom { get; private set; }
    public DateOnly PeriodTo { get; private set; }
    public string Status { get; private set; } = ProcessingStatuses.Pending;
    public string CurrentStep { get; private set; } = ProcessingSteps.Start;
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SatelliteProcessingRun()
    {
    }

    public static SatelliteProcessingRun Start(
        Guid fieldId,
        int geometryVersion,
        string country,
        string crop,
        DateOnly periodFrom,
        DateOnly periodTo,
        string requestedBy,
        DateTime now)
    {
        return new SatelliteProcessingRun
        {
            Id = Guid.NewGuid(),
            FieldId = fieldId,
            GeometryVersion = geometryVersion,
            Country = country,
            Crop = crop,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            Status = ProcessingStatuses.Running,
            CurrentStep = ProcessingSteps.Start,
            RequestedBy = requestedBy,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MoveTo(string step, DateTime now)
    {
        CurrentStep = step;
        UpdatedAt = now;
    }

    public void Complete(DateTime now)
    {
        Status = ProcessingStatuses.Completed;
        CurrentStep = ProcessingSteps.Completed;
        UpdatedAt = now;
    }

    public void Fail(DateTime now)
    {
        Status = ProcessingStatuses.Failed;
        UpdatedAt = now;
    }
}
