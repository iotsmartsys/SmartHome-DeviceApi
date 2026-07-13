namespace Core.Entities;

public sealed class DeviceMetrics
{
    public long UptimeMs { get; init; }
    public int CpuCores { get; init; }
    public decimal CpuPercent { get; init; }
    public decimal MemoryPercent { get; init; }
    public decimal TemperatureC { get; init; }
    public int FrequencyMhz { get; init; }
    public int Rssi { get; init; }
    public long LastDisconnectionUptimeMs { get; init; }
    public int LastDisconnectionReason { get; init; }
    public long ConnectionCount { get; init; }
}

public sealed class DeviceMetricsCurrent
{
    public string DeviceId { get; init; } = default!;
    public long UptimeMs { get; init; }
    public int CpuCores { get; init; }
    public decimal CpuPercent { get; init; }
    public decimal MemoryPercent { get; init; }
    public decimal TemperatureC { get; init; }
    public int FrequencyMhz { get; init; }
    public int Rssi { get; init; }
    public long LastDisconnectionUptimeMs { get; init; }
    public int LastDisconnectionReason { get; init; }
    public long ConnectionCount { get; init; }
    public DateTime ReceivedAt { get; init; }
}

public sealed class DeviceMetricsHistoryItem
{
    public long UptimeMs { get; init; }
    public decimal CpuPercent { get; init; }
    public decimal MemoryPercent { get; init; }
    public decimal TemperatureC { get; init; }
    public int FrequencyMhz { get; init; }
    public int Rssi { get; init; }
    public DateTime ReceivedAt { get; init; }
}

public sealed record DeviceMetricsHistoryPage(
    int Page,
    int PageSize,
    long TotalItems,
    IReadOnlyCollection<DeviceMetricsHistoryItem> Items);
