using System.Text;
using System.Text.Json;
using Incomming.Service.Core.Contracts.Publishers;
using Incomming.Service.Infrastructure.Factories;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Incomming.Service.Core.Implementations.Publishers;

internal class AmpqRabitmqPublisher(ILogger<AmpqRabitmqPublisher> logger, IOptions<RabbitMqConfiguration> settings, IChannel channel) : IPublisher
{
    public async Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken) where TEvent : class
    {
        var exchange = settings.Value.ExchangeName;
        logger.LogInformation($"Publicando evento para o tópico '{topic}' no exchange '{exchange}'");
        var props = new BasicProperties
        {
            Headers = new Dictionary<string, object?>
        {
            { "x-retry", 0 }
        }
        };
        object data = @event;
        string payload = JsonSerializer.Serialize(data);
        logger.LogInformation($"Evento serializado: {payload}");
        byte[] body = Encoding.UTF8.GetBytes(payload);
        await channel.BasicPublishAsync(exchange: exchange, routingKey: topic, mandatory: true, basicProperties: props, body: body, cancellationToken: cancellationToken);
        logger.LogInformation("Evento publicado com sucesso");
    }
}