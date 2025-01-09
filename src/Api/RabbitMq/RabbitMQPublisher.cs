using System.Text;
using System.Text.Json;
using Core.Contracts.Events;
using RabbitMQ.Client;

namespace SmartHome_Api.RabbitMq;

public class RabbitMQPublisher(IConnectionFactory factory, ILogger<RabbitMQPublisher> logger) : IEventPublisher, IAsyncDisposable
{
    readonly string queueName = "devices.register";
    private IConnection? _connection = default!;
    private IChannel? _channel = default!;

    async Task ConnectAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Conectando ao RabbitMQ");
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        logger.LogInformation("Conexão estabelecida com o RabbitMQ");
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        if (_connection is null)
        {

            await ConnectAsync(cancellationToken);
        }
        if (_channel is null)
        {
            throw new InvalidOperationException("Conexão com o RabbitMQ não foi estabelecida");
        }

        logger.LogInformation($"Publicando mensagem na fila {queueName}");
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(
            exchange: "amq.topic",
            routingKey: queueName,
            body: body,
            cancellationToken);

        logger.LogInformation($"Mensagem publicada na fila {queueName}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}