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

    public async Task UpdateAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Atualizando setting: {name}", settings.Name);
            const string query = @"UPDATE Settings SET Value = @Value, Description = @Description WHERE Name = @Name";
            var command = new CommandDefinition(query, settings, cancellationToken: cancellationToken);
            int rowsAffected = await connection.ExecuteAsync(command);
            if (rowsAffected == 0)
                throw new KeyNotFoundException($"Setting com nome '{settings.Name}' não encontrada");

            logger.LogInformation("Setting atualizada com sucesso: {name}", settings.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao atualizar setting: {name}", settings.Name);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

        public async Task SetValueAsync(string name, string value, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Definindo valor da setting: {name}", name);
                const string query = @"UPDATE Settings SET Value = @Value WHERE Name = @Name";
                var parameters = new { Name = name, Value = value };
                var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
                int rowsAffected = await connection.ExecuteAsync(command);
                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"Setting com nome '{name}' não encontrada");
    
                logger.LogInformation("Valor da setting definido com sucesso: {name}", name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao definir valor da setting: {name}", name);
                throw;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
}
