using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class CapabilityRepository(ILogger<CapabilityRepository> logger, IDbConnection connection) : ICapabilityRepository, IRepository
{
    public async Task AddAsync(string device_id, IEnumerable<Capability> capabilities)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>("SELECT Id FROM Devices WHERE DeviceId = @device_id", new { device_id }, transaction);
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            foreach (var capability in capabilities)
            {
                logger.LogInformation("Adicionando capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
                const string sql = CapabilityQuery.AddForDevice;
                await connection.ExecuteAsync(sql, new
                {
                    DeviceId = idDevice,
                    capability.Name,
                    capability.Description,
                    capability.Type,
                    capability.Value,
                    capability.Owner
                }, transaction);

                logger.LogInformation("Capability {capabilityName} adicionada para o device {deviceId}", capability.Name, device_id);

                foreach (var platform in capability.Platforms)
                {
                    logger.LogInformation("Adicionando capability {capabilityName} para o device {deviceId} na plataforma {platformName}", capability.Name, device_id, platform);
                    const string sqlPlatform = CapabilityQuery.AddPlatformToCapability;
                    await connection.ExecuteAsync(sqlPlatform, new
                    {
                        DeviceId = idDevice,
                        capability.Name,
                        platform
                    }, transaction);
                    logger.LogInformation("Capability {capabilityName} adicionada para o device {deviceId} na plataforma {platformName}", capability.Name, device_id, platform);
                }
            }
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao adicionar capabilities para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task<Capability?> GetByIdAsync(string device_id, int id, CancellationToken cancellationToken)
    {
        const string sql = CapabilityQuery.GetByDeviceAndId;
        var command = new CommandDefinition(sql, new { device_id, id = id }, cancellationToken: cancellationToken);
        return (await GetAllAsync(command)).FirstOrDefault();

    }

    public async Task<IEnumerable<Capability>> GetByDeviceAndNameAsync(string device_id, CancellationToken cancellationToken, params string[] capability_name)
    {
        const string sql = CapabilityQuery.GetByDeviceAndName;
        var command = new CommandDefinition(sql, new { device_id, capability_name = capability_name }, cancellationToken: cancellationToken);
        return await GetAllAsync(command);
    }

    public async Task<IEnumerable<Capability>> GetCapabilitiesByDeviceAsync(string device_id, CapabilityFind? capabilityQuery, CancellationToken cancellationToken)
    {
        try
        {
            connection.Open();
            IFindCapabilityQueryBuilder queryBuilder = CapabilityQueryBuilderFactory.Create(capabilityQuery ?? new CapabilityFind());

            queryBuilder.WithCancellationToken(cancellationToken);
            queryBuilder
                .WithDeviceId(device_id)
                .OrderBy("Name");

            var command = queryBuilder.Build();
            return await GetAllAsync(command);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao buscar capabilities para o device {deviceId}", device_id);
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    async Task<IEnumerable<Capability>> GetAllAsync(CommandDefinition command)
    {
        List<Capability> capabilitiesSelecteds = [];
        return await connection.QueryAsync<Capability, Platform, Capability>(command: command, (capability, platform) =>
        {
            var capabilitySelected = capabilitiesSelecteds.FirstOrDefault(c => c.Id == capability.Id);
            if (capabilitySelected == null)
            {
                capabilitiesSelecteds.Add(capability);
                capabilitySelected = capability;
            }

            if (platform != null)
                capabilitySelected.AddPlatform(platform.Name);

            return capabilitySelected;
        });
    }

    public async Task DeleteAsync(string device_id, int id)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>("SELECT Id FROM Devices WHERE DeviceId = @device_id", new { device_id }, transaction);
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            logger.LogInformation("Removendo o relacionamento de plataforma para a capability {id} para o device {deviceId}", id, device_id);
            const string sqlPlatform = CapabilityQuery.RemovePlatformFromCapability;
            await connection.ExecuteAsync(sqlPlatform, new
            {
                DeviceId = idDevice,
                CapabilityId = id
            }, transaction);
            logger.LogInformation("Relacionamento de plataforma removido para a capability {id} para o device {deviceId}", id, device_id);

            logger.LogInformation("Removendo capability {id} para o device {deviceId}", id, device_id);
            const string sql = CapabilityQuery.RemoveFromDevice;
            await connection.ExecuteAsync(sql, new
            {
                DeviceId = idDevice,
                id = id
            }, transaction);

            logger.LogInformation("Capability {id} removida para o device {deviceId}", id, device_id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao remover capabilities para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task UpdateAsync(string device_id, Capability capability)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idDevice = await connection.ExecuteScalarAsync<int>("SELECT Id FROM Devices WHERE DeviceId = @device_id", new { device_id }, transaction);
            if (idDevice == 0)
            {
                logger.LogWarning("Device {deviceId} not found", device_id);
                throw new NotFoundExceptionDomain($"Device {device_id} not found");
            }

            logger.LogInformation("Atualizando capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
            const string sql = CapabilityQuery.UpdateForDevice;
            await connection.ExecuteAsync(sql, new
            {
                id = capability.Id,
                DeviceId = idDevice,
                capability.Name,
                capability.Description,
                capability.Type,
                capability.Value,
                capability.Owner,
                capability.Active
            }, transaction);

            logger.LogInformation("Capability {capabilityName} atualizada para o device {deviceId}", capability.Name, device_id);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ao atualizar capability para o device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}