using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SatellitePipeline.Infrastructure;
using SatellitePipeline.Infrastructure.Hosting;
using System;

namespace SatellitePipeline.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSatellitePipeline(builder.Configuration);
            builder.Services.AddHostedService<OutboxDispatcherHostedService>();
            builder.Services.AddControllers();

            builder.Services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Satellite Pipeline API",
                    Version = "v1",
                    Description = "Satellite imagery processing pipeline"
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
            {
                app.UseSwagger();
                app.UseSwaggerUI(swagger =>
                {
                    swagger.SwaggerEndpoint("/swagger/v1/swagger.json", "Satellite Pipeline API v1");
                    swagger.RoutePrefix = "swagger";
                });
            }

            app.MapControllers();

            var urls = app.Urls;
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                foreach (var url in urls)
                    Console.WriteLine($"Satellite Pipeline API listening on {url}");
                Console.WriteLine("Swagger UI: /swagger");
            });

            app.Run();
        }
    }
}
