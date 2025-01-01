using System;
using System.Data;
using Core;
using Core.Contracts.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Data.SqlServer.Repositories;

internal class DeviceRepository(ILogger<DeviceRepository> logger,IDbConnection connection) : IDeviceRepository
{
    public async Task<IEnumerable<Device>> GetDevicesAsync()
    {
        logger.LogInformation($"Getting devices from database. Connection string: {connection.ConnectionString}");
        const string sql = @"
        SELECT
            d.DeviceId DeviceId,
            d.Name deviceName,
            d.LastActive LastActive,
            d.Status state,
            dc.Name Name,    
            c.Name type,
            c.ActuatorMode mode,
            dc.value
        FROM devices d
            INNER JOIN DeviceCapabilities dc ON d.Id = dc.DeviceId
            INNER JOIN Capabilities c ON dc.CapabilityId = c.Id ";

        var devices = await connection.QueryAsync<Device, Capability, Device>(
            sql,
            (device, capability) =>
            {
                logger.LogInformation("Mapping device {deviceId} capability {capabilityName}", device.DeviceId, capability.Name);
               return device.AddCapability(capability);
            },
            splitOn: "Name"
        );

        logger.LogInformation($"Found {devices.Count()} devices");

        return devices;
    }

    
}