using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatellitePipeline.Infrastructure;

namespace SatellitePipeline.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var runBackfill = string.Equals(
                Environment.GetEnvironmentVariable("RUN_BACKFILL_ON_STARTUP"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSatellitePipelineWorker(context.Configuration, runBackfill);
                })
                .Build()
                .Run();
        }
    }
}
