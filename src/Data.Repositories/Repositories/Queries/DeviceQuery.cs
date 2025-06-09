using Core.Entities;

namespace Data.Repositories;

internal static class DeviceQuery
{
    public const string GetDevices = @"
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
                dc.UpdatedAt,
                dp.Id ,
                dp.Name,
                dp.Value,
                dp.Description Description,
                p.Id,
                p.Name Name
            FROM Devices d
                LEFT JOIN Capabilities dc ON d.Id = dc.DeviceId
                LEFT JOIN CapabilityTypes c ON dc.CapabilityId = c.Id 
                LEFT JOIN DeviceProperties dp ON d.Id = dp.DeviceId
                LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
                LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id
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
            INSERT INTO Capabilities_RelationShip_Platforms (DeviceCapabilityId, PlatformId)
            VALUES(
                (SELECT Id FROM Capabilities WHERE Name = @capabilityName AND DeviceId = @idDevice LIMIT 1), (SELECT Id FROM Platforms WHERE Name = @platformName LIMIT 1));";

    public const string UpdateDevice = @"
        UPDATE Devices 
        SET 
            Name = @Name, 
            Description = @Description, 
            LastActive = NOW(), 
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
