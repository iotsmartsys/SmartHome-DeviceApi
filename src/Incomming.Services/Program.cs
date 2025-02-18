using Incomming.Service;
using Incomming.Service.Infrastructure.DI;

var builder = Host.CreateApplicationBuilder(args);

await builder.Services
    .AddAutomationAsync(builder.Configuration, cancellationToken: default);

builder.Services
    .AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();