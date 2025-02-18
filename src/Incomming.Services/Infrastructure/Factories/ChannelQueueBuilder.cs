using RabbitMQ.Client;

namespace Incomming.Service.Infrastructure.Factories;

internal class ChannelQueueBuilder(IChannel channel) : IChanelQueueBuilder
{
    private string? _queueName;
    private string? _exchangeName;
    private string _routingKey = string.Empty;
    private bool _durable = true;
    private IDictionary<string, object?> _arguments = new Dictionary<string, object?>();

    public IChanelQueueBuilder WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IChanelQueueBuilder WithExchangeName(string exchangeName)
    {
        _exchangeName = exchangeName;
        return this;
    }

    public IChanelQueueBuilder WithRoutingKey(string routingKey)
    {
        _routingKey = routingKey;
        return this;
    }

    public async Task<IChannel> DeclareQueueAsync(CancellationToken cancellationToken)
    {
        if (_queueName is null)
            throw new InvalidOperationException("Queue name is required");
        if (_exchangeName is null)
            throw new InvalidOperationException("Exchange name is required");

        if (_routingKey is null)
            _routingKey = _queueName;

        await channel.QueueDeclareAsync(queue: _queueName, durable: _durable, exclusive: false, autoDelete: false, arguments: _arguments, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey, cancellationToken: cancellationToken);

        return channel;
    }

    public async Task<IChannel> DeclareQueueWithDlqAsync(CancellationToken cancellationToken)
    {
        if (_queueName is null)
            throw new InvalidOperationException("Queue name is required");
        if (_exchangeName is null)
            throw new InvalidOperationException("Exchange name is required");

        if (_routingKey is null)
            _routingKey = _queueName;

        string exchangeDlq = $"{_queueName}.dlx";
        string dlqName = $"{_queueName}.dlq";

        await channel.ExchangeDeclareAsync(exchangeDlq, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue: dlqName, exchange: exchangeDlq, routingKey: dlqName, cancellationToken: cancellationToken);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", exchangeDlq },
            { "x-dead-letter-routing-key", dlqName },
            { "x-message-ttl", 60000 }
        };

        await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _routingKey, cancellationToken: cancellationToken);

        return channel;
    }
}
