using SatellitePipeline.Application;
using SatellitePipeline.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var pipeline = PipelineComposition.Create();

builder.Services.AddSingleton(pipeline);
builder.Services.AddSingleton<ICommandBus>(pipeline.CommandBus);

var app = builder.Build();

app.MapPost("/satellite-processing", async (
    StartSatelliteProcessingRequest request,
    ICommandBus commandBus,
    CancellationToken ct) =>
{
    await commandBus.Send(new StartSatelliteProcessingCommand(
        Guid.NewGuid(),
        Guid.NewGuid(),
        request.FieldId,
        request.GeometryVersion,
        request.Country,
        request.Crop,
        request.PeriodFrom,
        request.PeriodTo,
        "api"), ct);

    return Results.Accepted();
});

app.MapPost("/outbox/drain", async (PipelineComposition pipeline, CancellationToken ct) =>
{
    var dispatched = await pipeline.OutboxDispatcher.DrainOnce(ct);
    return Results.Ok(new { dispatched });
});

app.MapGet("/runs", (PipelineComposition pipeline) =>
{
    return Results.Ok(new
    {
        runs = pipeline.Db.Runs.Select(x => new
        {
            x.Id,
            x.FieldId,
            x.GeometryVersion,
            x.Country,
            x.Crop,
            x.PeriodFrom,
            x.PeriodTo,
            x.Status,
            x.CurrentStep
        }),
        artifacts = pipeline.Db.Artifacts.Select(x => new
        {
            x.RunId,
            x.ArtifactType,
            x.IndexType,
            x.MinioKey,
            x.Status
        }),
        outbox = pipeline.Db.OutboxMessages.Select(x => new
        {
            x.Type,
            x.Status,
            x.RetryCount,
            x.LastError
        })
    });
});

app.Run();

public sealed record StartSatelliteProcessingRequest(
    Guid FieldId,
    int GeometryVersion,
    string Country,
    string Crop,
    DateOnly PeriodFrom,
    DateOnly PeriodTo);
