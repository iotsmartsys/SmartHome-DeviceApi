namespace Incomming.Service.Infrastructure.Factories;

public class RabbitMqConfiguration
{
    public string HostName { get; set; }= "iotserver.local";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "central-iot-automation.service";
    public string ExchangeName { get; set; } = "amq.topic";
    public int RetryCount { get; set; } = 10;
}
