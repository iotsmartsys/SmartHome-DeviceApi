
namespace Core.Contracts.Events;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
/*
{
    "action": "update",
    "context": "capability",
    "capabilities": [
        {
            "capability_name": "portaoGaragemState",
            "description": "Simula o sensor de abertura do port\u00E3o da garagem",
            "owner": "esp32-3BFC3F",
            "type": "Switch",
            "mode": null,
            "value": "false",
            "platforms": [
                "Arduino IoT Cloud"
            ],
            "value_type": null
        }
    ]
}
*/



