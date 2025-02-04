using System;
using System.Data;
using Core.Contracts.Repositories;
using Core.Entities;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class DeviceRepository(ILogger<DeviceRepository> logger, IDbConnection connection) : IDeviceRepository
{
    public async Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"Getting devices from database. Connection string: {connection.ConnectionString}");
        var command = new DeviceQueryBuilder()
        .WithCancellationToken(cancellationToken)
        .Build();
        List<Device> devicesSelecteds = [];
        var devices = await connection.QueryAsync<Device, Capability, Property?, Platform?, Device>(
            command,
            (device, capability, property, platform) =>
            {
                logger.LogInformation("Mapping device {deviceId} capability {capabilityName}", device.DeviceId, capability.Name);
                var deviceSelected = devicesSelecteds.FirstOrDefault(d => d.DeviceId == device.DeviceId);
                if (deviceSelected == null)
                {
                    deviceSelected = device;
                    devicesSelecteds.Add(deviceSelected);
                }

                var capInList = deviceSelected.AddCapability(capability);
                if (capInList != null && platform != null)
                {
                    capInList.AddPlatform(platform.Name);
                }

                deviceSelected.AddProperty(property!);

                return deviceSelected;
            },
            splitOn: "Id"
        );

        logger.LogInformation($"Found {devices.Count()} devices");

        return devicesSelecteds;
    }
    public async Task<Device?> GetDeviceAsync(string device_id, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Getting devices from database. Connection string: {connection.ConnectionString}");
        var command = new DeviceQueryBuilder()
            .WithCancellationToken(cancellationToken)
            .WithDeviceId(device_id)
                .Build();
        Device? result = null;
        var devices = await connection.QueryAsync<Device, Capability, Property?, Platform?, Device>(
          command: command,
          map: (device, capability, property, platform) =>
            {
                result ??= device;
                logger.LogInformation("Mapping device {deviceId} capability {capabilityName}", device.DeviceId, capability.Name);

                logger.LogInformation("Adding capability {capabilityName} to device {deviceId}", capability.Name, device.DeviceId);
                var capInList = result.AddCapability(capability);
                logger.LogInformation("Capability {capabilityName} added to device {deviceId}", capInList.Name, device.DeviceId);
                if (capInList != null && platform != null)
                {
                    logger.LogInformation("Adding platform {platformName} to capability {capabilityName} of device {deviceId}", platform.Name, capability.Name, device.DeviceId);
                    capInList.AddPlatform(platform.Name);
                }

                result.AddProperty(property!);

                return result;
            },
            splitOn: "Id, Id, Id, Id"
        );

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
}
