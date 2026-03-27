public interface INotificationEventFacade
{
    Task PublishStateChangeAsync(CapabilityStateChangeEvent @event, CancellationToken cancellationToken);
    Task PublishUpdateChangeAsync(CapabilityAddedOrUpdateEvent @event, CancellationToken cancellationToken);
    Task PublishDeviceAddedAsync(DeviceAddedEvent @event, CancellationToken cancellationToken);
    Task PublishDeviceRemovedAsync(DeviceRemovedEvent @event, CancellationToken cancellationToken);
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent;
}
public interface IEvent;
public record class CapabilityStateChangeEvent(string device_id, string capability_name, string value) : IEvent;
public record class CapabilityAddedOrUpdateEvent(string uid) : IEvent;
public record class DeviceAddedEvent(string device_id) : IEvent;
public record class DeviceRemovedEvent(string device_id) : IEvent;