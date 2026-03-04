using Core.Entities;

namespace Data.Repositories;

internal static class DeviceQuery
{
    public const string GetAllDevices = @"       
            SELECT
                d.Id ,
                d.DeviceId DeviceId,
                d.Name Name,
                d.description,
                d.LastActive LastActive,
                d.PowerOn PowerOn,
                d.Status state,
                d.MacAddress,
                d.IpAddress,
                d.CommunicationTypeId Protocol,
                d.Platform,
                d.Active IsActive,
                dp.Id ,
                dp.Name,
                dp.Value,
                dp.Description Description,
                ds.Id ,
                ds.Name,
                ds.Value,
                ds.Description Description
            FROM Devices d 
                LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
                LEFT JOIN DeviceSettings ds ON d.Id = ds.DeviceId
            WHERE 1 = 1
        ";
    public const string GetDevicesWithCapabilities = @"
        SELECT
            /* Device */
            d.Id AS Id,
            d.DeviceId AS DeviceId,
            d.Name AS Name,
            d.Description AS Description,
            d.LastActive AS LastActive,
            d.PowerOn AS PowerOn,
            d.Status AS State,
            d.MacAddress AS MacAddress,
            d.IpAddress AS IpAddress,
            d.CommunicationTypeId AS Protocol,
            d.Platform AS Platform,
            /* Capability */
            c.Id AS Id,
            c.Name AS Name,
            c.Description AS Description,
            ct.Name AS Type,
            ct.ActuatorMode AS Mode,
            c.Value AS Value,
            c.DeviceOwner AS Owner,
            ct.DataType AS DataType,
            c.UpdatedAt AS UpdatedAt,
            c.Active AS Active,
            c.IconName AS IconName,
            c.IconActiveColor AS IconActiveColor,
            c.IconInactiveColor AS IconInactiveColor,
            /* Property */
            dp.Id AS Id,
            dp.Name AS Name,
            dp.Value AS Value,
            dp.Description AS Description,
            /* Platform */
            p.Id AS Id,
            p.Name AS Name,
            /* Settings */
            ds.Id AS Id,
            ds.Name AS Name,
            ds.Value AS Value,
            ds.Description AS Description
        FROM Devices d
            LEFT JOIN Capabilities c ON d.Id = c.DeviceId
            LEFT JOIN CapabilityTypes ct ON c.CapabilityTypeId = ct.Id 
            LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
            LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON c.Id = dcrsp.CapabilityId 
            LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id
            LEFT JOIN DeviceSettings ds ON d.Id = ds.DeviceId
        WHERE 1 = 1
            ";

    public const string InsertDevice = @"
        INSERT INTO Devices (DeviceId, Name, Description, LastActive, Status, MacAddress, IpAddress, CommunicationTypeId, Platform)
        VALUES (@DeviceId, @Name, @Description, NOW(), @State, @MacAddress, @IpAddress, @Protocol, @Platform);
        SELECT LAST_INSERT_ID() AS NewId;
        ";
    public const string InsertCapability = @"
        INSERT INTO Capabilities (DeviceId, Name, DeviceOwner, CapabilityId, Value, Description)
        VALUES (@DeviceId, @Name, @Owner, (SELECT Id FROM CapabilityTypes WHERE Name = @Type LIMIT 1), @Value, @Description)
        ";

    public const string InsertProperty = @"
        INSERT INTO DeviceProperties (DeviceId, Name, Value)
        VALUES (@DeviceId, @Name, @Value)
        ";

    public const string InsertPlatform = @"
            INSERT INTO Capabilities_RelationShip_Platforms (CapabilityId, PlatformId)
            VALUES(
                (SELECT Id FROM Capabilities WHERE Name = @capabilityName AND DeviceId = @idDevice LIMIT 1), (SELECT Id FROM Platforms WHERE Name = @platformName LIMIT 1));";

    public const string UpdateDevice = @"
        UPDATE Devices 
        SET 
            Name = @Name, 
            Description = @Description, 
            LastActive = NOW(), 
            PowerOn = @PowerOn,
            Status = @State, 
            MacAddress = @MacAddress, 
            IpAddress = @IpAddress, 
            CommunicationTypeId = @Protocol, 
            Platform = @Platform
        WHERE Id = @Id;
        ";

    public const string DeleteDevice = @"
        DELETE FROM Devices WHERE DeviceId = @DeviceId;
        ";
    public const string DeleteDeviceCapabilities = @"
        DELETE FROM Capabilities WHERE DeviceId = @DeviceId;
        ";
    public const string DeleteProperties = @"
        DELETE FROM DeviceProperties WHERE DeviceId = @DeviceId;
        ";
}
