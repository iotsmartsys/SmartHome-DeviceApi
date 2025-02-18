using System.Text;
using System.Text.Json;
using Incomming.Service.Core.Contracts.Models;
using Incomming.Service.Core.Exceptions;
using Incomming.Service.Core.Models;
using Incomming.Service.Infrastructure.Factories;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Incomming.Service;

public partial class Worker(
    ILogger<Worker> logger,
    IOptions<RabbitMqConfiguration> options,
    IChannel channel,
    IChanelQueueBuilder chanelQueueBuilder,
    IServiceProvider serviceProvider)
        : BackgroundService
{
    private readonly string _queueName = options.Value.QueueName;
    private readonly RabbitMqConfiguration _rabitMqConfig = options.Value;
    int _retryCount => _rabitMqConfig.RetryCount;


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Criando canal com RabbitMQ");
        chanelQueueBuilder
            .WithQueueName(_queueName)
            .WithExchangeName(_rabitMqConfig.ExchangeName);

        await chanelQueueBuilder.DeclareQueueAsync(cancellationToken);

        logger.LogInformation("Configuração do RabbitMQ concluída");

        logger.LogInformation("Iniciando serviço");
        await base.StartAsync(cancellationToken);
        logger.LogInformation("Serviço iniciado");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation("Iniciando consumo de mensagens");
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                stoppingToken.ThrowIfCancellationRequested();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logger.LogInformation($"{DateTime.Now:dd/MM/yyyy HH:mm:ss.fff} Mensagem recebida: {0}", message);

                int deliveryCount = 0;
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry", out object? value))
                    deliveryCount = Convert.ToInt32(value);

                await ProcessMessageAsync(ea, body, message, deliveryCount, stoppingToken);
            };

            string consumerTag = await channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);
            logger.LogInformation("Consumidor criado com tag: {0}", consumerTag);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consumir mensagens");
        }
    }

    private async Task<int> ProcessMessageAsync(BasicDeliverEventArgs ea, byte[] body, string message, int deliveryCount, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var capabilityEvent = JsonSerializer.Deserialize<IncommingDevice>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (capabilityEvent is null)
                throw new InvalidEventException("Mensagem inválida");

            var command = new Command<IncommingDevice>(capabilityEvent);
            var response = await mediator.Send(command, stoppingToken);
            switch (response.Status)
            {
                case ResponseStatus.Success:
                case ResponseStatus.ThereIsNothingToBeDone:
                    logger.LogInformation($"DeliveryTag: {ea.DeliveryTag} > Mensagem processada com sucesso: {message}");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    logger.LogInformation("======MENSAGEM REMOVIDA DA FILA======");
                    break;
                case ResponseStatus.Invalid:
                    logger.LogWarning($"DeliveryTag: {ea.DeliveryTag} > Mensagem inválida: {message}");
                    await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, stoppingToken);
                    break;
                case ResponseStatus.Error:
                    logger.LogError($"DeliveryTag: {ea.DeliveryTag} > Erro ao processar mensagem: {message}");
                    throw new Exception(response.Message);
            }

            if (deliveryCount >= _retryCount)
                logger.LogInformation($"DeliveryTag: {ea.DeliveryTag} > Mensagem processada após {deliveryCount} tentativas de {_retryCount}");

        }
        catch (JsonException ex)
        {
            logger.LogError(ex, $"DeliveryTag: {ea.DeliveryTag} > Erro ao processar mensagem: {message}");
            logger.LogInformation("Rejeitando mensagem");

            await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
        }
        catch (InvalidEventException ex)
        {
            logger.LogError(ex, $"DeliveryTag: {ea.DeliveryTag} > Erro ao processar mensagem: {message}");
            logger.LogInformation($"DeliveryTag: {ea.DeliveryTag} > Rejeitando mensagem");

            await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"DeliveryTag: {ea.DeliveryTag} > Erro ao processar mensagem: {message}");
            // Em caso de falha, incremente o contador e reenvie a mensagem
            deliveryCount++;

            await RequeueRetray(ea, body, deliveryCount, stoppingToken);
        }

        return deliveryCount;
    }

    private async Task RequeueRetray(BasicDeliverEventArgs ea, byte[] body, int deliveryCount, CancellationToken stoppingToken)
    {
        var props = new BasicProperties
        {
            Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object?>(){
                            { "x-retry", deliveryCount},
                            { "x-max-retry", _retryCount}
                        }
        };

        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        // Reenvia a mensagem para a fila
        await channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: true, basicProperties: props, body: body, cancellationToken: stoppingToken);

        // Rejeita a mensagem atual sem reencaminhá-la
        await channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        logger.LogInformation("Serviço encerrado");
    }
}
