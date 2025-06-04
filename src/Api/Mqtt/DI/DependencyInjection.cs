

using MQTTnet;

public static class DependencyInjection
{
    public static async Task<IServiceCollection> AddMqttClientAsync(this IServiceCollection services, IConfiguration configuration, CancellationToken cancellationToken)
    {
        string? mqttHost = configuration["Mqtt:Host"];
        if (string.IsNullOrEmpty(mqttHost))
            throw new ArgumentException("MQTT host is not configured.");
        if (int.TryParse(configuration["Mqtt:Port"], out int mqttPort) == false)
            throw new ArgumentException("MQTT port is not configured or invalid.");
        string? userName = configuration["Mqtt:User"];
        if (string.IsNullOrEmpty(userName))
            throw new ArgumentException("MQTT user is not configured.");
        string? password = configuration["Mqtt:Password"];
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("MQTT password is not configured.");

        Console.WriteLine($"Connecting to MQTT broker at {mqttHost}:{mqttPort} with user {userName}");
        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttHost, mqttPort)
            .WithClientId("DeviceApi")
            .WithCredentials(userName, password)
            .WithCleanSession()
            .Build();

        var mqttFactory = new MqttClientFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        Console.WriteLine("Connecting to MQTT broker...");
        mqttClient.ConnectedAsync += e =>
        {
            Console.WriteLine("Connected to MQTT broker successfully.");
            return Task.CompletedTask;
        };
        mqttClient.DisconnectedAsync += e =>
        {
            Console.WriteLine("Disconnected from MQTT broker.");
            return Task.CompletedTask;
        };
        await mqttClient.ConnectAsync(mqttOptions, cancellationToken);

        services.AddSingleton<IMqttClient>(provider =>
        {
            return mqttClient;
        });

        return services;
    }
}