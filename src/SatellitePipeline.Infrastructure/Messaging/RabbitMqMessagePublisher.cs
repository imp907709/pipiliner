using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SatellitePipeline.Application;
using SatellitePipeline.Infrastructure.Messaging;

namespace SatellitePipeline.Infrastructure.Messaging;

public sealed class RabbitMqMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly InfrastructureOptions options;
    private readonly ILogger<RabbitMqMessagePublisher> logger;
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMqMessagePublisher(InfrastructureOptions options, ILogger<RabbitMqMessagePublisher> logger)
    {
        this.options = options;
        this.logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = options.RabbitMqHost,
            Port = options.RabbitMqPort,
            UserName = options.RabbitMqUser,
            Password = options.RabbitMqPassword,
            DispatchConsumersAsync = true
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        channel.ExchangeDeclare(options.RabbitMqExchange, ExchangeType.Topic, durable: true);
    }

    public Task Publish(OutboxMessage outboxMessage, CancellationToken ct)
    {
        var body = Encoding.UTF8.GetBytes(outboxMessage.PayloadJson);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.Type = outboxMessage.Type;
        properties.MessageId = outboxMessage.Id.ToString();

        channel.BasicPublish(
            options.RabbitMqExchange,
            outboxMessage.Type,
            properties,
            body);

        logger.LogDebug("Published outbox message {MessageId} type {Type}", outboxMessage.Id, outboxMessage.Type);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }
}
