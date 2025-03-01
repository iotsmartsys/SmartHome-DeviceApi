using System.Text;
using System.Text.Json;
using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Incomming.Service.Core.Contracts.Models;
using Incomming.Service.Core.Exceptions;
using Incomming.Service.Core.Models;
using Incomming.Service.Infrastructure.Factories;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Incomming.Service;

public partial class Worker(ILogger<Worker> logger) : BackgroundService
{
        string DEVICE_ID = "3c03dd75-717d-47e5-b358-c3e592a53fad";
        string SECRET_KEY = "8?BAmeGpgAGMJJYu!wYobGVGY";
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // .WithCredentials("marceloc", "Ma522770")
        // .WithTcpServer("5edc7281737f43669750c30c332654b9.s1.eu.hivemq.cloud", 8883)
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var options = new HiveMQClientOptions()
        {
            ClientId = DEVICE_ID,
            UserName = "marceloc",
            Password = "Ma522770",
            Host = "5edc7281737f43669750c30c332654b9.s1.eu.hivemq.cloud",
            Port = 8883
        };
        var client = new HiveMQClient(options);
        var connectResult = await client.ConnectAsync();

        client.OnMessageReceived += (sender, args) =>
        {
            logger.LogInformation("Message Received: {}", args.PublishMessage.PayloadAsString);
            // Handle Message in args.PublishMessage
            Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString);

        };
        await client.SubscribeAsync("core/dg/entity/227489");
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
