using Incomming.Service.Core.Contracts.Facades;
using Incomming.Service.Core.Contracts.Publishers;
using Incomming.Service.Core.Implementations.Facades;
using Incomming.Service.Core.Implementations.Publishers;
using Incomming.Service.Infrastructure.Factories;
using RabbitMQ.Client;

namespace Incomming.Service.Infrastructure.DI;

public static class DIAutomationExtensions
{
    public static async Task<IServiceCollection> AddAutomationAsync(this IServiceCollection services, IConfiguration configuration, CancellationToken cancellationToken)
    {
        return await services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DIAutomationExtensions).Assembly))
            .AddScoped<IDeviceFacade, DeviceFace>()
            .AddSingleton<IChanelQueueBuilder, ChannelQueueBuilder>()
            .AddSingleton<IPublisher, AmpqRabitmqPublisher>()
            .AddAutomationApi(configuration)
            .AddRabbitMqAsync(configuration, cancellationToken);
    }

    private static async Task<IServiceCollection> AddRabbitMqAsync(this IServiceCollection services, IConfiguration configuration, CancellationToken cancellationToken)
    {
        return services;
        
        services.AddOptions<RabbitMqConfiguration>()
                    .Configure(options => configuration.GetSection("RabbitMQ").Bind(options));

        string? hostName = configuration.GetOrThrowIfNullOrEmpty("RabbitMQ:HostName");
        int port = configuration.GetIntOrThrowIfNullOrEmpty("RabbitMQ:Port");
        string? userName = configuration.GetOrThrowIfNullOrEmpty("RabbitMQ:UserName");
        string? password = configuration.GetOrThrowIfNullOrEmpty("RabbitMQ:Password");

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
        IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        services.AddSingleton(_ => connection);
        services.AddSingleton(_ => channel);

        return services;
    }

    private static IServiceCollection AddAutomationApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ClientDeviceApiSettings>()
                    .Configure(options => configuration.GetSection("ClientAutomation").Bind(options));

        string name = configuration.GetOrThrowIfNullOrEmpty("ClientAutomation:Name");
        services.AddHttpClient(name, client =>
        {
            string uriString = configuration.GetOrThrowIfNullOrEmpty("ClientAutomation:BaseUrl");

            client.BaseAddress = new Uri(uriString);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-key", configuration["ClientAutomation:ApiKey"]);

        });

        return services;
    }

    static string GetOrThrowIfNullOrEmpty(this IConfiguration configuration, string paramName)
    {
        string? value = configuration[paramName];
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(paramName);

        return value;
    }

    static int GetIntOrThrowIfNullOrEmpty(this IConfiguration configuration, string paramName)
    {
        string? value = configuration[paramName];
        if (int.TryParse(value, out int result))
            return result;

        throw new ArgumentNullException(paramName);
    }
}
