namespace Data.Repositories;

internal static class GroupQuery
{
    public const string GetAllGroups = @"
    SELECT 
        g.Id AS Id,
        g.Name AS Name,
        g.Activated AS IsActive,
        g.CreatedAt,
        g.UpdatedAt ,
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
        /* Icone */
        0 AS Id,
        g.IconName AS Name
    FROM `Groups` g
        LEFT JOIN Group_RelationShipCapabilities grsc ON g.Id = grsc.GroupId
        LEFT JOIN Capabilities c ON grsc.CapabilityId = c.Id  
        LEFT JOIN CapabilityTypes ct ON c.CapabilityTypeId = ct.Id
    WHERE 
        1 = 1
    ";

    public const string Insert = @"
    INSERT INTO `Groups` (Name, Activated, CreatedAt, UpdatedAt)
    VALUES (@name, @activated, NOW(), NOW());
    SELECT LAST_INSERT_ID() AS NewId;
    ";

    public const string Update = @"
    UPDATE `Groups`
    SET
        Name = @name,
        Activated = @activated,
        UpdatedAt = NOW(),
        IconName = @IconName
    WHERE Id = @id;
    ";
    public const string Delete = @"
    DELETE FROM `Groups`
    WHERE Id = @id;
    ";

    public const string InsertCapabilityForGroup = @"
    INSERT INTO `Group_RelationShipCapabilities` (GroupId, CapabilityId)
    VALUES (@groupId, @capabilityId);
    ";

    public const string DeleteAllCapabilityForGroup = @"
    DELETE FROM `Group_RelationShipCapabilities`
    WHERE GroupId = @groupId;
    ";

    public const string DeleteCapabilityForGroup = @"
    DELETE FROM `Group_RelationShipCapabilities`
    WHERE GroupId = @groupId AND CapabilityId = @capabilityId;
    ";
}
