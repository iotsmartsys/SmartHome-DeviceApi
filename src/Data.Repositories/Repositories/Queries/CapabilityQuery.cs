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
            dc.icon_name AS IconName,
            dc.icon_color_active AS IconActiveColor,
            dc.icon_color_inactive AS IconInactiveColor,
            crsp.Id,
            p.Name Platform,
            crsp.ReferenceId,
            g.Id,
            g.Name,
            g.IconName
        FROM Capabilities dc
            INNER JOIN CapabilityTypes ct ON dc.CapabilityId = ct.Id
            LEFT JOIN Group_RelationShipCapabilities gsc ON dc.Id = gsc.CapabilityId
            LEFT JOIN `Groups` g ON gsc.GroupId = g.Id  
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
            icon_name = @icon_name,
            icon_color_active = @icon_active_color,
            icon_color_inactive = @icon_inactive_color,
            CapabilityId = (SELECT Id FROM CapabilityTypes WHERE Name = @type LIMIT 1)
        WHERE
            Id = @id;
    ";

    public const string RemovePlatformFromCapability = @"
        DELETE FROM Capabilities_RelationShip_Platforms 
        WHERE DeviceCapabilityId = @CapabilityId;
            ";

    public const string RemoveGroupFromCapability = @"
        DELETE FROM Group_RelationShipCapabilities 
        WHERE CapabilityId = @CapabilityId;
            ";

    public const string UpdateValue = @"
        UPDATE Capabilities
        SET Value = @value, UpdatedAt = CURRENT_TIMESTAMP
        WHERE Name = @capability_name;
    ";

    public const string InsertPlatformToCapability = @"
        INSERT INTO Capabilities_RelationShip_Platforms (DeviceCapabilityId, PlatformId, ReferenceId)
        VALUES (@DeviceCapabilityId, (SELECT Id FROM Platforms WHERE Name = @Platform LIMIT 1), @ReferenceId);
    ";

    public const string RemoveHistory = @"DELETE FROM CapabilityHistory WHERE CapabilityId = @CapabilityId;";

    public const string InsertHistory = @"
        INSERT INTO CapabilityHistory (CapabilityId, Value, UpdatedAt)
        VALUES ((SELECT Id FROM Capabilities WHERE Name = @CapabilityName LIMIT 1), @Value, CURRENT_TIMESTAMP);
    ";

    public const string SelectHistory = @"
    SELECT 
        ch.UpdatedAt,
        ch.Value 
    FROM CapabilityHistory ch 
    WHERE 
        ch.CapabilityId = @CapabilityId
    -- Use MySQL-compatible datetime arithmetic. For MySQL: NOW() - INTERVAL @LastHours HOUR
    AND (@LastHours IS NULL OR ch.UpdatedAt >= (NOW() - INTERVAL @LastHours HOUR))
    AND (@DateStart IS NULL OR ch.UpdatedAt >= @DateStart)
    AND (@DateEnd IS NULL OR ch.UpdatedAt <= @DateEnd)
    ORDER BY ch.UpdatedAt DESC, ch.Id DESC

    ";
}
