namespace Incomming.Service.Core.Contracts.Publishers;

public interface IPublisher
{
    Task PublishAsync<TEvent>(string topic, TEvent @event,CancellationToken cancellationToken) where TEvent : class;
}