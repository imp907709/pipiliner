namespace SatellitePipeline.Infrastructure;

public sealed class InfrastructureOptions
{
    public string? PostgresConnectionString { get; set; }
    public string? RabbitMqHost { get; set; }
    public int RabbitMqPort { get; set; } = 6672;
    public string RabbitMqUser { get; set; } = "guest";
    public string RabbitMqPassword { get; set; } = "guest";
    public string RabbitMqExchange { get; set; } = "satellite.pipeline";
    public string RabbitMqQueue { get; set; } = "satellite.pipeline.events";
    public string? MinioEndpoint { get; set; }
    public string MinioAccessKey { get; set; } = "pipeline";
    public string MinioSecretKey { get; set; } = "pipeline123";
    public string MinioBucket { get; set; } = "satellite";

    public bool UsesPostgres => !string.IsNullOrWhiteSpace(PostgresConnectionString);
    public bool UsesRabbitMq => !string.IsNullOrWhiteSpace(RabbitMqHost);
}
