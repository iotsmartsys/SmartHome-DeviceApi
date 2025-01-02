using System;
using System.Data;
using Core.Entities;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.SqlServer.Repositories;

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
            dp.Id ,
            dp.Name,
            dp.Value
        FROM devices d
            INNER JOIN DeviceCapabilities dc ON d.Id = dc.DeviceId
            INNER JOIN Capabilities c ON dc.CapabilityId = c.Id 
            LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
            ";
        List<Device> devicesSelecteds = new();
        var devices = await connection.QueryAsync<Device, Capability, Property?, Device>(
            sql,
            (device, capability, property) =>
            {
                logger.LogInformation("Mapping device {deviceId} capability {capabilityName}", device.DeviceId, capability.Name);
                var deviceSelected = devicesSelecteds.FirstOrDefault(d => d.DeviceId == device.DeviceId);
                if (deviceSelected == null)
                {
                    deviceSelected = device;
                    devicesSelecteds.Add(deviceSelected);
                }

                deviceSelected.AddCapability(capability);
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
            dp.Id ,
            dp.Name,
            dp.Value
        FROM devices d
            INNER JOIN DeviceCapabilities dc ON d.Id = dc.DeviceId
            INNER JOIN Capabilities c ON dc.CapabilityId = c.Id 
            LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
            WHERE d.DeviceId = @device_id
            ";
        Device? result = null;
        var devices = await connection.QueryAsync<Device, Capability, Property?, Device>(
          sql: sql,
          param: new { device_id },
          map: (device, capability, property) =>
            {
                result ??= device;
                logger.LogInformation("Mapping device {deviceId} capability {capabilityName}", device.DeviceId, capability.Name);

                result.AddCapability(capability);
                result.AddProperty(property!);

                return result;
            },
            splitOn: "Id"
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
            logger.LogInformation("Creating device {deviceId}", entity.DeviceId);
            const string sql = @"
        INSERT INTO devices (DeviceId, Name,[Description], LastActive, Status, MacAddress, IpAddress, CommunicationTypeId, Platform)
        VALUES (@DeviceId, @Name,@Description, @LastActive, @State, @MacAddress, @IpAddress, @Protocol, @Platform)
        SELECT SCOPE_IDENTITY()
        ";

            entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity, transaction);
            logger.LogInformation("Device {deviceId} created", entity.DeviceId);

            const string capabilitySql = @"
        INSERT INTO DeviceCapabilities (DeviceId, Name, DeviceOwner, CapabilityId, Value, [Description])
        VALUES (@DeviceId, @Name, @Owner, (SELECT TOP 1 Id FROM Capabilities WHERE [Name] = @Type ), @Value, @Description)
        ";

            foreach (var capability in entity.Capabilities)
            {
                logger.LogInformation("Adding capability {capabilityName} to device {deviceId}", capability.Name, entity.DeviceId);
                await connection.ExecuteAsync(capabilitySql, new
                {
                    DeviceId = entity.Id,
                    capability.Name,
                    capability.Owner,
                    capability.Type,
                    capability.Value,
                    capability.Description
                }, transaction);
            }

            const string propertySql = @"
        INSERT INTO DeviceProperties (DeviceId, Name, Value)
        VALUES (@DeviceId, @Name, @Value)
        ";

            foreach (var property in entity.Properties)
            {
                logger.LogInformation("Adding property {propertyName} to device {deviceId}", property.Name, entity.DeviceId);
                await connection.ExecuteAsync(propertySql, new
                {
                    DeviceId = entity.Id,
                    property.Name,
                    property.Value
                }, transaction);
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
