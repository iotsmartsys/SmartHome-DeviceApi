
namespace Core.Contracts.Events;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
    Task PublishAsync<TEvent>(string queue, TEvent @event, CancellationToken cancellationToken);
}


