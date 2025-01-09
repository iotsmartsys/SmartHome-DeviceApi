
using Core.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using SmartHome_Api.RabbitMq;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var factory = new ConnectionFactory
            {
                // HostName = "rabbitmq",
                HostName = "iotserver.local",
                Port = 5672,
                UserName = "smarthomeiot",
                Password = "Smarthomeiot@123"
            };

            return factory;
        });
        services.AddSingleton<IEventPublisher, RabbitMQPublisher>();

        return services;
    }
}