namespace Data.Repositories;

internal static class CapabilityQuery
{
    public const string GetAllCapabilities = $@"
        SELECT
            c.Id AS Id,
            c.UID AS UID,
            c.Name AS Name,
            c.Description AS Description,
            ct.Name AS Type,
            ct.ActuatorMode AS Mode,
            c.Value AS Value,
            c.DeviceOwner AS Owner,
            d.DeviceId AS DeviceId,
            ct.DataType AS DataType,
            c.UpdatedAt AS UpdatedAt,
            c.Active AS Active,
            c.IconName AS IconName,
            c.IconActiveColor AS IconActiveColor,
            c.IconInactiveColor AS IconInactiveColor,
            crsp.Id AS Id,
            p.Name AS Platform,
            crsp.ReferenceId AS ReferenceId,
            g.Id AS Id,
            g.Name AS Name,
            g.IconName AS IconName,
            g.IconColor AS IconColor,
            sh.Id AS Id,
            sh.Name SmartHomeId,
            ctsh.Name AS Name,
            ctsh.Value AS Value
        FROM Capabilities c
            INNER JOIN CapabilityTypes ct ON c.CapabilityTypeId = ct.Id 
            INNER JOIN Devices d ON c.DeviceId = d.Id
            LEFT JOIN Group_RelationShipCapabilities gsc ON c.Id = gsc.CapabilityId
            LEFT JOIN `Groups` g ON gsc.GroupId = g.Id  
            LEFT JOIN Capabilities_RelationShip_Platforms crsp ON c.Id = crsp.CapabilityId
            LEFT JOIN Platforms p ON crsp.PlatformId = p.Id
            LEFT JOIN CapabilityTypesSmartHome ctsh ON ct.Id = ctsh.CapabilityTypeId
            LEFT JOIN SmartHome sh ON ctsh.SmartHomeId = sh.Id
        WHERE 1 = 1
        ";

    public const string GetByName = $@"{GetAllCapabilities} AND c.Name IN @capability_name";

    public const string GetById = $@"{GetAllCapabilities} AND c.Id = @id";

    public const string GetAllCapabilitiesActive = $@"{GetAllCapabilities} AND c.Active = true";

    public const string InsertCapability = @"
        INSERT INTO Capabilities (DeviceId, Name, Description, CapabilityTypeId, Value, deviceOwner, UID)
        VALUES (@DeviceId, @Name, @Description, (SELECT Id FROM CapabilityTypes WHERE Name = @Type LIMIT 1), @Value, @Owner, uuid_v4());
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
            DeviceOwner = @owner,
            Active = @active,
            IconName = @icon_name,
            IconActiveColor = @IconActiveColor,
            IconInactiveColor = @IconInactiveColor,
            CapabilityTypeId = (SELECT Id FROM CapabilityTypes WHERE Name = @type LIMIT 1)
        WHERE
            Id = @id;
    ";

    public const string RemovePlatformFromCapability = @"
        DELETE FROM Capabilities_RelationShip_Platforms 
        WHERE CapabilityId = @CapabilityId;
            ";

    public const string RemoveGroupFromCapability = @"
        DELETE FROM Group_RelationShipCapabilities 
        WHERE CapabilityId = @CapabilityId;
            ";

    public const string UpdateValue = @"
        UPDATE Capabilities
        SET Value = @value, UpdatedAt = CURRENT_TIMESTAMP
        WHERE Name = @capability_name
        LIMIT 1;
    ";

    public const string InsertPlatformToCapability = @"
        INSERT INTO Capabilities_RelationShip_Platforms (CapabilityId, PlatformId, ReferenceId)
        VALUES (@CapabilityId, (SELECT Id FROM Platforms WHERE Name = @Platform LIMIT 1), @ReferenceId);
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
