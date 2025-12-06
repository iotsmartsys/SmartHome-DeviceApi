using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class DeviceSettingsRepository(ILogger<DeviceSettingsRepository> logger, IDbConnection connection) : IDeviceSettingsRepository
{
    public async Task SaveAsync(string deviceId, IEnumerable<Settings> settings, CancellationToken cancellationToken)
    {
        connection.Open();
        var transaction = connection.BeginTransaction();
        try
        {
            logger.LogInformation("Salvando settings");
            foreach (var setting in settings)
            {
                var command = new CommandDefinition(DeviceSettingsQuery.InsertOrUpdateDeviceSettingsByDeviceId, new
                {
                    DeviceId = deviceId,
                    Name = setting.Name,
                    Value = setting.Value,
                    Description = setting.Description
                }, transaction: transaction, cancellationToken: cancellationToken);

                await connection.ExecuteAsync(command);
                logger.LogInformation("Settings salvas com sucesso");
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao salvar settings");
            transaction.Rollback();
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }

    public async Task<IEnumerable<Settings>> GetByDeviceIdAsync(string deviceId, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Buscando settings do device {DeviceId}", deviceId);
            var command = new CommandDefinition(DeviceSettingsQuery.GetDeviceSettingsByDeviceId, new
            {
                DeviceId = deviceId
            }, cancellationToken: cancellationToken);

            var result = await connection.QueryAsync<Settings>(command);
            logger.LogInformation("Settings do device {DeviceId} buscadas com sucesso", deviceId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao buscar settings do device {DeviceId}", deviceId);
            throw;
        }
        finally
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
        }
    }
}
