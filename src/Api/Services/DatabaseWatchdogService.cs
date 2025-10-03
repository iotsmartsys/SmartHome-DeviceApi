
using System.Data;
using Dapper;

public class DatabaseWatchdogService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseWatchdogService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2);
    private readonly int _failureThreshold = 3;
    private int _failureCount = 0;

    public DatabaseWatchdogService(IServiceProvider services, ILogger<DatabaseWatchdogService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Watchdog iniciado. Verificando conexão com o banco a cada {Interval} segundos.", _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
                try
                {
                    await db.ExecuteScalarAsync("SELECT 1");
                }
                finally
                {
                    if (db.State != ConnectionState.Closed)
                        db.Close();
                }
    
                _failureCount = 0;
                _logger.LogInformation("Conexão com o banco de dados verificada com sucesso.");
            }
            catch (Exception ex)
            {
                _failureCount++;
                _logger.LogError(ex, "Falha ao acessar o banco de dados. Tentativa {Count}/{Threshold}", _failureCount, _failureThreshold);

                if (_failureCount >= _failureThreshold)
                {
                    _logger.LogCritical("Número de falhas excedeu o limite. Encerrando o processo para que o Swarm reinicie o container.");
                    Environment.Exit(1);
                }
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
