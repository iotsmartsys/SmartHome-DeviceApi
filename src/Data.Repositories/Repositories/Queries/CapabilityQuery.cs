namespace Data.Repositories;

internal static class CapabilityQuery
{
    public const string GetAllCapabilities = $@"
        SELECT 
            dc.Id,
            dc.DeviceId, 
            dc.Name, 
            dc.Description, 
            ct.Name Type, 
            ct.ActuatorMode Mode, 
            dc.Value, 
            dc.deviceOwner Owner,
            ct.DataType,
            dc.UpdatedAt,
            dc.Active,
            crsp.Id,
            p.Name Platform,
            crsp.ReferenceId
        FROM Capabilities dc
            INNER JOIN CapabilityTypes ct ON dc.CapabilityId = ct.Id
            LEFT JOIN Capabilities_RelationShip_Platforms crsp ON dc.Id = crsp.DeviceCapabilityId
            LEFT JOIN Platforms p ON crsp.PlatformId = p.Id 
        WHERE 1 = 1      
        ";

    public const string GetByName = $@"{GetAllCapabilities} AND dc.Name IN @capability_name";

    public const string GetById = $@"{GetAllCapabilities} AND dc.Id = @id";

    public const string GetAllCapabilitiesActive = $@"{GetAllCapabilities} AND dc.Active = true";

    public const string InsertCapability = @"
        INSERT INTO Capabilities (DeviceId, Name, Description, CapabilityId, Value, deviceOwner)
        VALUES (@DeviceId, @Name, @Description, (SELECT Id FROM CapabilityTypes WHERE Name = @Type LIMIT 1), @Value, @Owner);
    ";

    public const string RemoveCapability = @"
        DELETE FROM Capabilities 
        WHERE 
            Id = @id;
    ";

    public const string UpdateForDevice = @"
        UPDATE Capabilities
        SET
            Value = @value,
            Name = @name,
            Description = @description,
            UpdatedAt = CURRENT_TIMESTAMP,
            deviceOwner = @owner,
            Active = @active,
            CapabilityId = (SELECT Id FROM CapabilityTypes WHERE Name = @type LIMIT 1)
        WHERE
            Id = @id;
    ";

    public const string RemovePlatformFromCapability = @"
        DELETE FROM Capabilities_RelationShip_Platforms 
        WHERE DeviceCapabilityId = @CapabilityId;
            ";

    public const string UpdateValue = @"
        UPDATE Capabilities
        SET Value = @value, UpdatedAt = CURRENT_TIMESTAMP
        WHERE Name = @capability_name;
    ";
}
