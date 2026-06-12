using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SatellitePipeline.Infrastructure.Messaging;

namespace SatellitePipeline.Infrastructure.Hosting;

public sealed class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly InfrastructureOptions options;
    private readonly MessageDispatcher dispatcher;
    private readonly ILogger<RabbitMqConsumerHostedService> logger;
    private IConnection? connection;
    private IModel? channel;

    public RabbitMqConsumerHostedService(
        InfrastructureOptions options,
        MessageDispatcher dispatcher,
        ILogger<RabbitMqConsumerHostedService> logger)
    {
        this.options = options;
        this.dispatcher = dispatcher;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
        channel.QueueDeclare(options.RabbitMqQueue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(options.RabbitMqQueue, options.RabbitMqExchange, routingKey: "#");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, eventArgs) =>
        {
            var type = eventArgs.BasicProperties.Type ?? eventArgs.RoutingKey;

            if (string.IsNullOrWhiteSpace(type))
            {
                logger.LogWarning("Received RabbitMQ message without type; skipping");
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                return;
            }

            var payloadJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                await dispatcher.DispatchAsync(type, payloadJson, stoppingToken);
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle RabbitMQ message type {Type}", type);
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(options.RabbitMqQueue, autoAck: false, consumer);
        logger.LogInformation("RabbitMQ consumer started on queue {Queue}", options.RabbitMqQueue);

        stoppingToken.Register(() =>
        {
            channel?.Close();
            connection?.Close();
        });

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
        base.Dispose();
    }
}
