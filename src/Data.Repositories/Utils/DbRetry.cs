using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Data.Repositories.Utils;

internal static class DbRetry
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, ILogger logger, CancellationToken cancellationToken, int maxRetries = 3)
    {
        var delayMs = 100;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (MySqlException ex) when (IsTransient(ex))
            {
                if (attempt >= maxRetries || cancellationToken.IsCancellationRequested)
                    throw;

                logger.LogWarning(ex, "DB transient error (attempt {attempt}/{max}). Retrying in {delay}ms", attempt, maxRetries, delayMs);
                await Task.Delay(delayMs, cancellationToken);
                delayMs = Math.Min(delayMs * 2, 1000); // backoff exponencial limitado
                continue;
            }
        }
    }

    public static async Task ExecuteAsync(Func<Task> action, ILogger logger, CancellationToken cancellationToken, int maxRetries = 3)
    {
        await ExecuteAsync(async () => { await action(); return true; }, logger, cancellationToken, maxRetries);
    }

    private static bool IsTransient(MySqlException ex)
    {
        if (ex is null) return false;
        if (ex.IsTransient) return true;

        // Fallback para mensagens comuns
        var msg = ex.Message ?? string.Empty;
        if (msg.Contains("Lost connection", StringComparison.OrdinalIgnoreCase)) return true;
        if (msg.Contains("Server has gone away", StringComparison.OrdinalIgnoreCase)) return true;
        if (msg.Contains("timeout", StringComparison.OrdinalIgnoreCase)) return true;
        if (msg.Contains("Got timeout reading communication packets", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
