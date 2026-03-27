using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

public class NotificationEventFacade(HttpClient httpClient, IOptions<NotificationEventOptions> options) : INotificationEventFacade
{
    private readonly NotificationEventOptions _config = options.Value;

    public async Task PublishStateChangeAsync(CapabilityStateChangeEvent @event, CancellationToken cancellationToken)
    {
        await PublishAsync(@event, cancellationToken);
    }

    public async Task PublishUpdateChangeAsync(CapabilityAddedOrUpdateEvent @event, CancellationToken cancellationToken)
    {
        await PublishAsync(@event, cancellationToken);
    }

    public async Task PublishDeviceAddedAsync(DeviceAddedEvent @event, CancellationToken cancellationToken)
    {
        await PublishAsync(@event, cancellationToken);
    }

    public async Task PublishDeviceRemovedAsync(DeviceRemovedEvent @event, CancellationToken cancellationToken)
    {
        await PublishAsync(@event, cancellationToken);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        string topic = @event switch
        {
            CapabilityStateChangeEvent => _config.CapabilityStateChangeTopic,
            CapabilityAddedOrUpdateEvent => _config.CapabilityAddedOrUpdateTopic,
            DeviceAddedEvent => _config.DeviceAddedTopic,
            DeviceRemovedEvent => _config.DeviceRemovedTopic,
            _ => throw new InvalidOperationException("Unsupported event type")
        };

        var payload = new
        {
            topic = topic,
            payload = JsonSerializer.Serialize((object)@event),
            qos = 0,
            retain = false
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(_config.MqttBrokerEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
public sealed class NotificationEventOptions
{
    public string MqttBrokerUrl { get; init; } = string.Empty;
    public string MqttBrokerEndpoint { get; init; } = string.Empty;
    public string Authorization { get; init; } = string.Empty;
    public string CapabilityStateChangeTopic { get; init; } = string.Empty;
    public string CapabilityAddedOrUpdateTopic { get; init; } = string.Empty;
    public string DeviceAddedTopic { get; init; } = string.Empty;
    public string DeviceRemovedTopic { get; init; } = string.Empty;
}
