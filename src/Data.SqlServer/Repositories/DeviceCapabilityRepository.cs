using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.SqlServer.Repositories;

internal class DeviceCapabilityRepository : IDeviceCapabilityRepository, IRepository
{
    private readonly ILogger<DeviceCapabilityRepository> logger;
    private readonly IDbConnection connection;

    public DeviceCapabilityRepository(ILogger<DeviceCapabilityRepository> logger, IDbConnection connection)
    {
        this.logger = logger;
        this.connection = connection;
    }

    public async Task AddForDeviceAsync(string device_id, IEnumerable<Capability> capabilities)
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
                const string sql = @"
            INSERT INTO DeviceCapabilities (DeviceId, CapabilityId, Name, [Description], Value, deviceOwner)
    VALUES(@DeviceId, (SELECT TOP 1 Id FROM Capabilities WHERE Name = @Type), @Name, @Description, @Value, @Owner)
            ";
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
                    const string sqlPlatform = @"
                    INSERT INTO DeviceCapabilities_RelationShip_Platforms (DeviceCapabilityId, PlatformId)
                    VALUES((SELECT TOP 1 Id FROM DeviceCapabilities WHERE DeviceId = @DeviceId AND Name = @Name), (SELECT TOP 1 Id FROM Platforms WHERE Name = @Platform))
                    ";
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

    public async Task<IEnumerable<Capability>> GetCapabilitiesByDeviceAsync(string device_id)
    {
        try
        {
            const string sql = @"
           SELECT 
                dc.Id,
                dc.DeviceId, 
                dc.Name, 
                dc.[Description], 
                c.Name Type, 
                c.ActuatorMode Mode, 
                dc.Value, 
                dc.deviceOwner Owner,
                c.DataType, 
                p.Id,
                p.Name 
            FROM DeviceCapabilities dc
                INNER JOIN Capabilities c ON dc.CapabilityId = c.Id
                INNER JOIN Devices d ON dc.DeviceId = d.Id
		        LEFT JOIN DeviceCapabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
		        LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id 
            WHERE d.DeviceId = @device_id
        ";

            List<Capability> capabilitiesSelecteds = [];
            return await connection.QueryAsync<Capability, Platform, Capability>(sql, (capability, platform) =>
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
            }, new { device_id });
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

    public async Task RemoveFromDeviceAsync(string device_id, IEnumerable<Capability> enumerable)
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

            foreach (var capability in enumerable)
            {
                logger.LogInformation("Removendo o relacionamento de plataforma para a capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
                const string sqlPlatform = @"
            DELETE FROM DeviceCapabilities_RelationShip_Platforms
            WHERE
                DeviceCapabilityId = (SELECT TOP 1 Id FROM DeviceCapabilities WHERE DeviceId = @DeviceId AND Name = @Name)
            ";
                await connection.ExecuteAsync(sqlPlatform, new
                {
                    DeviceId = idDevice,
                    capability.Name
                }, transaction);
                logger.LogInformation("Relacionamento de plataforma removido para a capability {capabilityName} para o device {deviceId}", capability.Name, device_id);

                logger.LogInformation("Removendo capability {capabilityName} para o device {deviceId}", capability.Name, device_id);
                const string sql = @"
            DELETE FROM DeviceCapabilities 
            WHERE 
                DeviceId = @DeviceId AND 
                CapabilityId = (SELECT TOP 1 Id FROM Capabilities WHERE Name = @Type) AND
                Name = @Name
            ";
                await connection.ExecuteAsync(sql, new
                {
                    DeviceId = idDevice,
                    capability.Name,
                    capability.Type
                }, transaction);

                logger.LogInformation("Capability {capabilityName} removida para o device {deviceId}", capability.Name, device_id);
            }
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

    public async Task UpdateForDeviceAsync(string device_id, Capability capability)
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
            const string sql = @"
            UPDATE DeviceCapabilities
            SET
                Value = @Value
            WHERE
                DeviceId = @DeviceId AND
                Name = @Name
            ";
            await connection.ExecuteAsync(sql, new
            {
                DeviceId = idDevice,
                capability.Name,
                capability.Value
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
