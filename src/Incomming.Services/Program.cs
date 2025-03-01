using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using Incomming.Service;
using Incomming.Service.Infrastructure.DI;

var builder = Host.CreateApplicationBuilder(args);

// await builder.Services
//     .AddAutomationAsync(builder.Configuration, cancellationToken: default);

// builder.Services
//     .AddHostedService<Worker>();

string DEVICE_ID = "3c03dd75-717d-47e5-b358-c3e592a53fad";
var options = new HiveMQClientOptions()
{
    UserName = "marceloc",
    Password = "Ma522770",
    Host = "5edc7281737f43669750c30c332654b9.s1.eu.hivemq.cloud",
    Port = 8883,
    UseTLS = true,
     WebSocketServer = "wss://5edc7281737f43669750c30c332654b9.s1.eu.hivemq.cloud:8884/mqtt",
      UseWebSocket = true,
      ClientId = "testecsharp",
      CleanStart = true,
};
var client = new HiveMQClient(options);
var connectResult = await client.ConnectAsync();

Console.WriteLine("\r\n");
Console.WriteLine("Connected: {0}", connectResult.ReasonString);
Console.WriteLine("\r\n");
Console.WriteLine("ReasonCode: {0}", connectResult.ReasonCode);
Console.WriteLine("\r\n");
Console.WriteLine("ResponseInformation: {0}", connectResult.ResponseInformation);
Console.WriteLine("\r\n");

if (connectResult.SessionPresent)
{
    Console.WriteLine("Session Present");
}
else
{
    Console.WriteLine("Session Not Present");
}

client.OnMessageReceived += (sender, args) =>
{
    // Handle Message in args.PublishMessage
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString);

};
var subscribeResult = await client.SubscribeAsync("core/dg/entity/227489");

foreach (var item in subscribeResult.Subscriptions)
{
    Console.WriteLine("ReasonCode: {0}", item.SubscribeReasonCode);
    Console.WriteLine("\r\n");
    Console.WriteLine("UnsubscribeReasonCode: {0}", item.UnsubscribeReasonCode);
    Console.WriteLine("\r\n");
}

var host = builder.Build();
await host.RunAsync();

