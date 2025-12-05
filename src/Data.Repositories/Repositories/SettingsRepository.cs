using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class SettingsRepository(ILogger<SettingsRepository> logger, IDbConnection connection) : ISettingsRepository
{
    public async Task<IEnumerable<Settings>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Buscando todas as settings");
            const string query = @"SELECT Id, Name, Value, Description FROM Settings";
            var command = new CommandDefinition(query, cancellationToken: cancellationToken);
            IEnumerable<Settings> settings = await connection.QueryAsync<Settings>(command);
            logger.LogInformation("Settings encontradas: {count}", settings.AsList().Count);

            return settings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar settings");
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
