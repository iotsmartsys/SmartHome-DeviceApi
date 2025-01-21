using System;
using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.Repositories;

internal class DeviceRepository(ILogger<DeviceRepository> logger, IDbConnection connection) : IDeviceRepository
{
    public async Task<IEnumerable<Device>> GetDevicesAsync()
    {
        logger.LogInformation($"Getting devices from database. Connection string: {connection.ConnectionString}");
        const string sql = @"
        SELECT
                d.Id ,
                d.DeviceId DeviceId,
                d.Name Name,
                d.description,
                d.LastActive LastActive,
                d.Status state,
                d.MacAddress,
                d.IpAddress,
                d.CommunicationTypeId Protocol,
                d.Platform,
                dc.Id,
                dc.Name Name,
                dc.Description,
                dc.DeviceOwner Owner, 
                c.Name type,
                c.ActuatorMode mode,
                dc.value,
                c.DataType,
                dp.Id ,
                dp.Name,
                dp.Value,
                p.Id,
                p.Name Name
            FROM Devices d
                INNER JOIN Capabilities dc ON d.Id = dc.DeviceId
                INNER JOIN CapabilityTypes c ON dc.CapabilityId = c.Id 
                LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
                LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
                LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id 
            ";
        List<Device> devicesSelecteds = [];
        var devices = await connection.QueryAsync<Device, Capability, Property?, Platform?, Device>(
            sql,
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
    public async Task<Device?> GetDeviceAsync(string device_id)
    {
        logger.LogInformation($"Getting devices from database. Connection string: {connection.ConnectionString}");
        const string sql = @"
        SELECT
                d.Id ,
                d.DeviceId DeviceId,
                d.Name Name,
                d.description,
                d.LastActive LastActive,
                d.Status state,
                d.MacAddress,
                d.IpAddress,
                d.CommunicationTypeId Protocol,
                d.Platform,
                dc.Id,
                dc.Name Name,
                dc.Description,
                dc.DeviceOwner Owner, 
                c.Name type,
                c.ActuatorMode mode,
                dc.value,
                c.DataType, 
                dp.Id ,
                dp.Name,
                dp.Value,
                p.Id,
                p.Name
            FROM Devices d
                INNER JOIN Capabilities dc ON d.Id = dc.DeviceId
                INNER JOIN CapabilityTypes c ON dc.CapabilityId = c.Id 
                LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
                LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
                LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id 
            WHERE d.DeviceId = @device_id
            ";
        Device? result = null;
        var devices = await connection.QueryAsync<Device, Capability, Property?, Platform?, Device>(
          sql: sql,
          param: new { device_id },
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

    public async Task CreateAsync(Device entity)
    {
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            logger.LogInformation("Ciando device {deviceId}", entity.DeviceId);
            const string sql = @"
        INSERT INTO Devices (DeviceId, Name, Description, LastActive, Status, MacAddress, IpAddress, CommunicationTypeId, Platform)
        VALUES (@DeviceId, @Name, @Description, NOW(), @State, @MacAddress, @IpAddress, @Protocol, @Platform);
        SELECT LAST_INSERT_ID() AS NewId;
        ";

            entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity, transaction);
            logger.LogInformation("Device {deviceId} criado com sucesso", entity.DeviceId);

            const string capabilitySql = @"
        INSERT INTO Capabilities (DeviceId, Name, DeviceOwner, CapabilityId, Value, Description)
        VALUES (@DeviceId, @Name, @Owner, (SELECT Id FROM CapabilityTypes WHERE Name = @Type LIMIT 1), @Value, @Description)
        ";

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

            const string propertySql = @"
        INSERT INTO DeviceProperties (DeviceId, Name, Value)
        VALUES (@DeviceId, @Name, @Value)
        ";

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

            const string platformSql = @"
            INSERT INTO Capabilities_RelationShip_Platforms (DeviceCapabilityId, PlatformId)
            VALUES(
                (SELECT Id FROM Capabilities WHERE Name = @capabilityName AND DeviceId = @idDevice LIMIT 1), (SELECT Id FROM Platforms WHERE Name = @platformName LIMIT 1));";

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
