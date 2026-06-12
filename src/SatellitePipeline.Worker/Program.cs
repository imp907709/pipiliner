using SatellitePipeline.Application;
using SatellitePipeline.Infrastructure;

var pipeline = PipelineComposition.Create();

Console.WriteLine("Quartz-like job starts satellite backfill...");
await pipeline.BackfillJob.Execute(CancellationToken.None);

for (var i = 0; i < 20; i++)
{
    var dispatched = await pipeline.OutboxDispatcher.DrainOnce(CancellationToken.None);
    if (dispatched == 0)
        break;
}

Console.WriteLine();
Console.WriteLine("Runs:");
foreach (var run in pipeline.Db.Runs)
{
    Console.WriteLine($"- {run.Id} field={run.FieldId} status={run.Status} step={run.CurrentStep}");
}

Console.WriteLine();
Console.WriteLine("Artifacts:");
foreach (var artifact in pipeline.Db.Artifacts)
{
    Console.WriteLine($"- {artifact.ArtifactType}/{artifact.IndexType}: {artifact.MinioKey}");
}

Console.WriteLine();
Console.WriteLine("Outbox:");
foreach (var message in pipeline.Db.OutboxMessages)
{
    Console.WriteLine($"- {message.Type}: {message.Status}");
}
