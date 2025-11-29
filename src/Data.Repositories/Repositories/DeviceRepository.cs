using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class DeviceRepository(ILogger<DeviceRepository> logger, IDbConnection connection) : IDeviceRepository
{
    public async Task<IEnumerable<Device>> GetDevicesAsync(DeviceFind? find, CancellationToken cancellationToken)
    {
        var command = new DeviceQueryBuilder()
        .WithCancellationToken(cancellationToken)
        .WithFind(find)
        .Build();
        return await GetByCommandAsync(command);
    }

    private async Task<IEnumerable<Device>> GetByCommandAsync(CommandDefinition command)
    {
        List<Device> devicesSelecteds = [];
        var devices = await connection.QueryAsync<Device, Property?, Settings?, Device>(
            command,
            (device, property, settings) =>
            {
                var deviceSelected = devicesSelecteds.FirstOrDefault(d => d.DeviceId == device.DeviceId);
                if (deviceSelected == null)
                {
                    deviceSelected = device;
                    devicesSelecteds.Add(deviceSelected);
                }

                deviceSelected.AddProperty(property!);
                deviceSelected.AddSetting(settings!);

                return deviceSelected;
            },
            splitOn: "Id"
        );

        logger.LogInformation($"Found {devices.Count()} devices");

        return devicesSelecteds;
    }

    public async Task<Device?> GetDeviceAsync(string device_id, CancellationToken cancellationToken)
    {
        var command = new DeviceQueryBuilder(DeviceQuery.GetDevicesWithCapabilities)
            .WithCancellationToken(cancellationToken)
            .WithDeviceId(device_id)
                .Build();
        Device? result = null;
        var devices = await GetByCommandAsync(command);
        result = devices.FirstOrDefault();

        logger.LogInformation($"Found {devices.Count()} devices");
        return result;
    }

    public async Task CreateAsync(Device entity, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Ciando device {deviceId}", entity.DeviceId);
            const string sql = DeviceQuery.InsertDevice;

            entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity, transaction);
            logger.LogInformation("Device {deviceId} criado com sucesso", entity.DeviceId);

            const string capabilitySql = DeviceQuery.InsertCapability;

            foreach (var capability in entity.Capabilities)
            {
                logger.LogInformation("Adicionando capability {capabilityName} ao device {deviceId}", capability.Name, entity.DeviceId);
                await connection.ExecuteAsync(capabilitySql, new
                {
                    DeviceId = entity.Id,
                    capability.Name,
                    capability.Owner,
                    capability.Type,
                    capability.Value,
                    capability.Description
                }, transaction);
                logger.LogInformation("Capability {capabilityName} adicionada ao device {deviceId}", capability.Name, entity.DeviceId);
            }

            const string propertySql = DeviceQuery.InsertProperty;

            foreach (var property in entity.Properties)
            {
                logger.LogInformation("Adicionando propriedade {propertyName} ao device {deviceId}", property.Name, entity.DeviceId);
                await connection.ExecuteAsync(propertySql, new
                {
                    DeviceId = entity.Id,
                    property.Name,
                    property.Value
                }, transaction);
                logger.LogInformation("Propriedade {propertyName} adicionada ao device {deviceId}", property.Name, entity.DeviceId);
            }

            const string platformSql = DeviceQuery.InsertPlatform;

            var capabilitiesWithPlatforms = entity.Capabilities.Where(c => c.Platforms.Any()).ToArray();
            foreach (var capability in capabilitiesWithPlatforms)
            {
                foreach (var platform in capability.Platforms)
                {
                    logger.LogInformation("Adding platform {platformName} to capability {capabilityName} of device {deviceId}", platform, capability.Name, entity.DeviceId);
                    await connection.ExecuteAsync(platformSql, new { capabilityName = capability.Name, idDevice = entity.Id, platformName = platform }, transaction);
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating device {deviceId}", entity.DeviceId);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Updating device {deviceId}", device.DeviceId);
            const string sql = DeviceQuery.UpdateDevice;

            var command = new CommandDefinition(sql, device, transaction, cancellationToken: cancellationToken);
            logger.LogInformation("Executing update command for device {deviceId}", device.DeviceId);

            await connection.ExecuteAsync(command);
            logger.LogInformation("Device {deviceId} updated successfully", device.DeviceId);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating device {deviceId}", device.DeviceId);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }

    }

    public async Task DeleteAsync(string device_id, CancellationToken cancellationToken)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation($"Identificando device {device_id} para exclusão");
            var existingDevice = await GetDeviceAsync(device_id, cancellationToken);
            if (existingDevice == null)
            {
                logger.LogWarning("Device {deviceId} não encontrado para exclusão", device_id);
                return;
            }

            logger.LogInformation("Excluindo as propriedades do device {deviceId}", device_id);
            const string deletePropertiesSql = DeviceQuery.DeleteProperties;
            await connection.ExecuteAsync(deletePropertiesSql, new { DeviceId = existingDevice.Id }, transaction);
            logger.LogInformation("Propriedades do device {deviceId} excluídas com sucesso", device_id);

            logger.LogInformation("Excluindo as capacidades do device {deviceId}", device_id);
            const string deleteCapabilitiesSql = DeviceQuery.DeleteDeviceCapabilities;
            await connection.ExecuteAsync(deleteCapabilitiesSql, new { DeviceId = existingDevice.Id }, transaction);
            logger.LogInformation("Capacidades do device {deviceId} excluídas com sucesso", device_id);

            logger.LogInformation("Excluindo o device {deviceId}", device_id);
            const string sql = DeviceQuery.DeleteDevice;
            var command = new CommandDefinition(sql, new { DeviceId = device_id }, transaction, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
            logger.LogInformation("Device {deviceId} excluído com sucesso", device_id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting device {deviceId}", device_id);
            transaction.Rollback();
            throw;
        }
        finally
        {
            connection.Close();
        }
    }
}
