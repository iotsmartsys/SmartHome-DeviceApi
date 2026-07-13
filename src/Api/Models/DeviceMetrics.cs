using System.ComponentModel.DataAnnotations;
using Core.Entities;
using Newtonsoft.Json;

namespace Api.Models;

public sealed class DeviceMetricsRequest : IValidatableObject
{
    [JsonProperty("device_id")]
    [Required(AllowEmptyStrings = false)]
    public string? device_id { get; init; }

    [JsonProperty("uptime_ms")]
    [Required]
    public long? uptime_ms { get; init; }

    [JsonProperty("cpu_cores")]
    [Required]
    public int? cpu_cores { get; init; }

    [JsonProperty("cpu_percent")]
    [Required]
    public decimal? cpu_percent { get; init; }

    [JsonProperty("memory_percent")]
    [Required]
    public decimal? memory_percent { get; init; }

    [JsonProperty("temperature_c")]
    [Required]
    public decimal? temperature_c { get; init; }

    [JsonProperty("frequency_mhz")]
    [Required]
    public int? frequency_mhz { get; init; }

    [JsonProperty("network")]
    [Required]
    public DeviceNetworkMetricsRequest? network { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(device_id)) yield return Error("device_id é obrigatório", nameof(device_id));
        if (uptime_ms < 0) yield return Error("uptime_ms deve ser maior ou igual a zero", nameof(uptime_ms));
        if (cpu_cores <= 0) yield return Error("cpu_cores deve ser maior que zero", nameof(cpu_cores));
        if (cpu_percent is < 0 or > 100) yield return Error("cpu_percent deve estar entre 0 e 100", nameof(cpu_percent));
        if (memory_percent is < 0 or > 100) yield return Error("memory_percent deve estar entre 0 e 100", nameof(memory_percent));
        if (temperature_c is < -100 or > 200) yield return Error("temperature_c deve estar entre -100 e 200", nameof(temperature_c));
        if (frequency_mhz <= 0) yield return Error("frequency_mhz deve ser maior que zero", nameof(frequency_mhz));
    }

    public DeviceMetrics ToEntity() => new()
    {
        UptimeMs = uptime_ms!.Value,
        CpuCores = cpu_cores!.Value,
        CpuPercent = cpu_percent!.Value,
        MemoryPercent = memory_percent!.Value,
        TemperatureC = temperature_c!.Value,
        FrequencyMhz = frequency_mhz!.Value,
        Rssi = network!.rssi!.Value,
        LastDisconnectionUptimeMs = network.last_disconnection_uptime_ms!.Value,
        LastDisconnectionReason = network.disconnection_reason!.Value,
        ConnectionCount = network.connection_count!.Value
    };

    private static ValidationResult Error(string message, string member) => new(message, [member]);
}

public sealed class DeviceNetworkMetricsRequest : IValidatableObject
{
    [Required] public int? rssi { get; init; }
    [Required] public long? last_disconnection_uptime_ms { get; init; }
    [Required] public int? disconnection_reason { get; init; }
    [Required] public long? connection_count { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (rssi is < -150 or > 0) yield return Error("network.rssi deve estar entre -150 e 0", nameof(rssi));
        if (last_disconnection_uptime_ms < 0) yield return Error("network.last_disconnection_uptime_ms deve ser maior ou igual a zero", nameof(last_disconnection_uptime_ms));
        if (disconnection_reason < 0) yield return Error("network.disconnection_reason deve ser maior ou igual a zero", nameof(disconnection_reason));
        if (connection_count < 0) yield return Error("network.connection_count deve ser maior ou igual a zero", nameof(connection_count));
    }

    private static ValidationResult Error(string message, string member) => new(message, [member]);
}

public sealed record DeviceNetworkMetricsResponse(
    int rssi,
    long last_disconnection_uptime_ms,
    int disconnection_reason,
    long connection_count);

public sealed record DeviceMetricsCurrentResponse(
    string device_id,
    long uptime_ms,
    int cpu_cores,
    decimal cpu_percent,
    decimal memory_percent,
    decimal temperature_c,
    int frequency_mhz,
    DeviceNetworkMetricsResponse network,
    DateTime received_at)
{
    public static DeviceMetricsCurrentResponse FromEntity(DeviceMetricsCurrent value) => new(
        value.DeviceId, value.UptimeMs, value.CpuCores, value.CpuPercent, value.MemoryPercent,
        value.TemperatureC, value.FrequencyMhz,
        new(value.Rssi, value.LastDisconnectionUptimeMs, value.LastDisconnectionReason, value.ConnectionCount),
        value.ReceivedAt);
}

public sealed record DeviceMetricsHistoryItemResponse(
    long uptime_ms,
    decimal cpu_percent,
    decimal memory_percent,
    decimal temperature_c,
    int frequency_mhz,
    int rssi,
    DateTime received_at);

public sealed record DeviceMetricsHistoryResponse(
    int page,
    int page_size,
    long total_items,
    int total_pages,
    IReadOnlyCollection<DeviceMetricsHistoryItemResponse> items);
