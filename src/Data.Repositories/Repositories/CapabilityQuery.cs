namespace Data.Repositories;

internal static class CapabilityQuery
{
    public const string GetByDeviceAndNameAsync = @"
           SELECT 
                dc.Id,
                dc.DeviceId, 
                dc.Name, 
                dc.Description, 
                c.Name Type, 
                c.ActuatorMode Mode, 
                dc.Value, 
                dc.deviceOwner Owner,
                c.DataType, 
                p.Id,
                p.Name 
            FROM Capabilities dc
                INNER JOIN CapabilityTypes c ON dc.CapabilityId = c.Id
                INNER JOIN Devices d ON dc.DeviceId = d.Id
                LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
                LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id 
            WHERE d.DeviceId = @device_id AND dc.Name IN @capability_name";

    public const string GetCapabilitiesByDeviceAsync = @"
            SELECT 
                dc.Id,
                dc.DeviceId, 
                dc.Name, 
                dc.Description, 
                c.Name Type, 
                c.ActuatorMode Mode, 
                dc.Value, 
                dc.deviceOwner Owner,
                c.DataType, 
                p.Id,
                p.Name 
            FROM Capabilities dc
                INNER JOIN CapabilityTypes c ON dc.CapabilityId = c.Id
                INNER JOIN Devices d ON dc.DeviceId = d.Id
                LEFT JOIN Capabilities_RelationShip_Platforms dcrsp ON dc.Id = dcrsp.DeviceCapabilityId 
                LEFT JOIN Platforms p ON dcrsp.PlatformId = p.Id              
        ";
    
    public const string AddForDeviceAsync = @"
        INSERT INTO Capabilities (DeviceId, Name, Description, CapabilityId, Value, deviceOwner)
        VALUES (@DeviceId, @Name, @Description, (SELECT Id FROM CapabilityTypes WHERE Name = @Type LIMIT 1), @Value, @Owner);
    ";

    public const string RemoveFromDeviceAsync = @"
        DELETE FROM Capabilities 
        WHERE 
            DeviceId = @DeviceId 
            AND Name = @Name;
    ";

    public const string AddPlatformToCapability = @"
                        INSERT INTO Capabilities_RelationShip_Platforms (DeviceCapabilityId, PlatformId)
                        VALUES (
                            (SELECT Id FROM Capabilities WHERE DeviceId = @DeviceId AND Name = @Name LIMIT 1),
                            (SELECT Id FROM Platforms WHERE Name = @Platform LIMIT 1)
                        );
                    ";

    public const string UpdateForDeviceAsync = @"
        UPDATE Capabilities
        SET
            Value = @Value
        WHERE
            DeviceId = @DeviceId
            AND Name = @Name;
    ";

    public const string RemovePlatformFromCapability = @"
                DELETE FROM Capabilities_RelationShip_Platforms 
                WHERE DeviceCapabilityId IN
                    (
                        SELECT 
                            Id 
                        FROM Capabilities DC 
                        WHERE 
                            DC.DeviceId = @DeviceId AND 
                            DC.Name = @Name
                    );
            ";
}