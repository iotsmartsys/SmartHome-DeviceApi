using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MQTTnet;

[Route("api/v1/mqtt/commands")]
[ApiController]
public class CommandsController(ILogger<CommandsController> logger) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendCommandAsync([FromBody] Command command, IMqttClient mqttClient, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received command: {command}", command);
        if (command == null)
        {
            logger.LogWarning("Received null command.");
            return BadRequest("Command cannot be null.");
        }
        if (string.IsNullOrEmpty(command.device_id) || string.IsNullOrEmpty(command.capability_name))
        {
            logger.LogWarning("Invalid command received: {command}", command);
            return BadRequest("Device ID and capability name are required.");
        }
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"device/{command.device_id}/command")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
            .Build();


        if (!mqttClient.IsConnected)
        {
            logger.LogWarning("MQTT client is not connected.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "MQTT client is not connected.");
        }
        logger.LogInformation("Publishing command to MQTT topic: {topic}", message.Topic);
        logger.LogInformation("Command payload: {payload}", message.Payload);
        await mqttClient.PublishAsync(message, cancellationToken);
        logger.LogInformation("Command published successfully.");
        return Accepted();
    }
}
public record class Command(string device_id, string capability_name, string value);