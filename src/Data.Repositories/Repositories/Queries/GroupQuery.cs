namespace Data.Repositories;

internal static class GroupQuery
{
    public const string GetAllGroups = @"
    SELECT 
        g.Id,
        g.Name,
        g.Activated,
        g.CreatedAt,
        g.UpdatedAt ,
        c.Id,
        c.Name,
        c.Description,
        0 AS Id,
        g.IconName AS Name
    FROM `Groups` g
        LEFT JOIN Group_RelationShipCapabilities grsc ON g.Id = grsc.GroupId
        LEFT JOIN Capabilities c ON grsc.CapabilityId = c.Id  
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