using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal sealed class DeviceMetricsRepository(
    ILogger<DeviceMetricsRepository> logger,
    IDbConnection connection) : IDeviceMetricsRepository
{
    private const string FindDeviceIdSql = """
        SELECT Id FROM Devices WHERE DeviceId = @ExternalDeviceId LIMIT 1;
        """;

    public async Task<bool> SaveAsync(string externalDeviceId, DeviceMetrics metrics, CancellationToken cancellationToken)
    {
        var deviceId = await FindDeviceIdAsync(externalDeviceId, cancellationToken);
        if (deviceId is null)
        {
            logger.LogWarning("Dispositivo {DeviceId} não encontrado para persistência de métricas", externalDeviceId);
            return false;
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            var parameters = new
            {
                DeviceId = deviceId.Value,
                metrics.UptimeMs,
                metrics.CpuCores,
                metrics.CpuPercent,
                metrics.MemoryPercent,
                metrics.TemperatureC,
                metrics.FrequencyMhz,
                metrics.Rssi,
                metrics.LastDisconnectionUptimeMs,
                metrics.LastDisconnectionReason,
                metrics.ConnectionCount
            };

            const string upsertSql = """
                INSERT INTO DeviceMetricsCurrent
                    (DeviceId, UptimeMs, CpuCores, CpuPercent, MemoryPercent, TemperatureC,
                     FrequencyMhz, Rssi, LastDisconnectionUptimeMs, LastDisconnectionReason,
                     ConnectionCount, ReceivedAt)
                VALUES
                    (@DeviceId, @UptimeMs, @CpuCores, @CpuPercent, @MemoryPercent, @TemperatureC,
                     @FrequencyMhz, @Rssi, @LastDisconnectionUptimeMs, @LastDisconnectionReason,
                     @ConnectionCount, CURRENT_TIMESTAMP(3))
                ON DUPLICATE KEY UPDATE
                    UptimeMs = VALUES(UptimeMs), CpuCores = VALUES(CpuCores),
                    CpuPercent = VALUES(CpuPercent), MemoryPercent = VALUES(MemoryPercent),
                    TemperatureC = VALUES(TemperatureC), FrequencyMhz = VALUES(FrequencyMhz),
                    Rssi = VALUES(Rssi), LastDisconnectionUptimeMs = VALUES(LastDisconnectionUptimeMs),
                    LastDisconnectionReason = VALUES(LastDisconnectionReason),
                    ConnectionCount = VALUES(ConnectionCount), ReceivedAt = CURRENT_TIMESTAMP(3);
                """;
            const string historySql = """
                INSERT INTO DeviceMetricsHistory
                    (DeviceId, UptimeMs, CpuPercent, MemoryPercent, TemperatureC,
                     FrequencyMhz, Rssi, ReceivedAt)
                VALUES
                    (@DeviceId, @UptimeMs, @CpuPercent, @MemoryPercent, @TemperatureC,
                     @FrequencyMhz, @Rssi, CURRENT_TIMESTAMP(3));
                """;

            await connection.ExecuteAsync(new CommandDefinition(upsertSql, parameters, transaction, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(historySql, parameters, transaction, cancellationToken: cancellationToken));
            transaction.Commit();
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha ao persistir métricas do dispositivo {DeviceId}; executando rollback", externalDeviceId);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<DeviceMetricsCurrent?> GetCurrentAsync(string externalDeviceId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT D.DeviceId, M.UptimeMs, M.CpuCores, M.CpuPercent, M.MemoryPercent,
                   M.TemperatureC, M.FrequencyMhz, M.Rssi, M.LastDisconnectionUptimeMs,
                   M.LastDisconnectionReason, M.ConnectionCount, M.ReceivedAt
            FROM Devices D
            INNER JOIN DeviceMetricsCurrent M ON M.DeviceId = D.Id
            WHERE D.DeviceId = @ExternalDeviceId
            LIMIT 1;
            """;
        return await connection.QuerySingleOrDefaultAsync<DeviceMetricsCurrent>(new CommandDefinition(
            sql, new { ExternalDeviceId = externalDeviceId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeviceExistsAsync(string externalDeviceId, CancellationToken cancellationToken) =>
        await FindDeviceIdAsync(externalDeviceId, cancellationToken) is not null;

    public async Task<DeviceMetricsHistoryPage> GetHistoryAsync(string externalDeviceId, DateTime? from,
        DateTime? to, int page, int pageSize, CancellationToken cancellationToken)
    {
        const string countSql = """
            SELECT COUNT(*)
            FROM DeviceMetricsHistory H INNER JOIN Devices D ON D.Id = H.DeviceId
            WHERE D.DeviceId = @ExternalDeviceId
              AND (@From IS NULL OR H.ReceivedAt >= @From)
              AND (@To IS NULL OR H.ReceivedAt <= @To);
            """;
        const string itemsSql = """
            SELECT H.UptimeMs, H.CpuPercent, H.MemoryPercent, H.TemperatureC,
                   H.FrequencyMhz, H.Rssi, H.ReceivedAt
            FROM DeviceMetricsHistory H INNER JOIN Devices D ON D.Id = H.DeviceId
            WHERE D.DeviceId = @ExternalDeviceId
              AND (@From IS NULL OR H.ReceivedAt >= @From)
              AND (@To IS NULL OR H.ReceivedAt <= @To)
            ORDER BY H.ReceivedAt DESC
            LIMIT @PageSize OFFSET @Offset;
            """;
        var parameters = new { ExternalDeviceId = externalDeviceId, From = from, To = to, PageSize = pageSize, Offset = (page - 1) * pageSize };
        var total = await connection.ExecuteScalarAsync<long>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var items = (await connection.QueryAsync<DeviceMetricsHistoryItem>(new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken))).AsList();
        return new DeviceMetricsHistoryPage(page, pageSize, total, items);
    }

    private async Task<int?> FindDeviceIdAsync(string externalDeviceId, CancellationToken cancellationToken) =>
        await connection.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            FindDeviceIdSql, new { ExternalDeviceId = externalDeviceId }, cancellationToken: cancellationToken));
}
