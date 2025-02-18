using RabbitMQ.Client;

namespace Incomming.Service.Infrastructure.Factories;

public interface IChanelQueueBuilder
{
    IChanelQueueBuilder WithQueueName(string queueName);
    IChanelQueueBuilder WithExchangeName(string exchangeName);
    IChanelQueueBuilder WithRoutingKey(string routingKey);
    Task<IChannel> DeclareQueueAsync(CancellationToken cancellationToken);
    Task<IChannel> DeclareQueueWithDlqAsync(CancellationToken cancellationToken);
}
