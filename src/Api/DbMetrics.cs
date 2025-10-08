using System.Diagnostics;
using System.Diagnostics.Metrics;
using MySqlConnector;
using Dapper;

public static class DbMetrics
{
    public static readonly Meter Meter = new("SmartHomeApi.Metrics", "1.0.0");
    public static readonly Histogram<double> DbQueryDurationMs = Meter.CreateHistogram<double>("db_query_duration_ms");
    public static readonly Counter<long> DbTimeouts = Meter.CreateCounter<long>("db_timeouts_total");
    public static readonly Counter<long> DbErrors = Meter.CreateCounter<long>("db_errors_total");
}

public class InstrumentedDb
{
    private readonly string _connStr;
    private static readonly ActivitySource _activity = new("SmartHome-Api.db");

    public InstrumentedDb(string connStr) => _connStr = connStr;

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, int? timeoutSeconds = null)
    {
        var sw = Stopwatch.StartNew();
        using var span = _activity.StartActivity("mysql.query", ActivityKind.Client);
        span?.SetTag("db.system", "mysql");
        span?.SetTag("db.operation", "query");
        span?.SetTag("db.statement", sql);        // cuidado: pode conter PII; para prod você pode mascarar
        span?.SetTag("db.timeout.s", timeoutSeconds ?? 0);

        try
        {
            await using var conn = new MySqlConnection(_connStr);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<T>(new CommandDefinition(
                sql, param, commandTimeout: timeoutSeconds));

            return result;
        }
        catch (MySqlException ex) when (ex.Number == (int)MySqlErrorCode.CommandTimeoutExpired || ex.InnerException is TimeoutException)
        {
            DbMetrics.DbTimeouts.Add(1);
            span?.SetStatus(ActivityStatusCode.Error, $"timeout: {ex.Message}");
            span?.AddException(ex);
            throw;
        }
        catch (Exception ex)
        {
            DbMetrics.DbErrors.Add(1);
            span?.SetStatus(ActivityStatusCode.Error, ex.Message);
            span?.AddException(ex);
            throw;
        }
        finally
        {
            sw.Stop();
            DbMetrics.DbQueryDurationMs.Record(sw.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("db.name", "SmartHome-Devices"),
                new KeyValuePair<string, object?>("service.name", "SmartHome-Api"));
        }
    }
}