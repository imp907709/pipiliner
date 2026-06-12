using System;

namespace SatellitePipeline.Api
{
    public sealed record StartSatelliteProcessingRequest(
        Guid FieldId,
        int GeometryVersion,
        string Country,
        string Crop,
        DateOnly PeriodFrom,
        DateOnly PeriodTo);
}
