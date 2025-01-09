
using Core.Contracts.Events;
using RabbitMQ.Client;
using SmartHome_Api.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"]!,
                Port = int.Parse(configuration["RabbitMq:Port"]!),
                UserName = configuration["RabbitMq:User"]!,
                Password = configuration["RabbitMq:Password"]!
            };

            return factory;
        });
        services.AddSingleton<IEventPublisher, RabbitMQPublisher>();

        return services;
    }
}