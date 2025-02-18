using RabbitMQ.Client;

namespace Incomming.Service.Infrastructure.Factories;

internal class RabbitMqConnectFactory(RabbitMqConfiguration configuration)
{
    public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = configuration.HostName,
            UserName = configuration.UserName,
            Password = configuration.Password
        };

        return factory.CreateConnectionAsync(cancellationToken);
    }
}
