namespace Data.Repositories;

internal static class CapabilityTypeQuery
{
    public const string SelectBase = @"
        SELECT 
            Id, 
            Name, 
            ActuatorMode,
            DataType,
            DynamicComputedValue AS ComputedValue,
            value_symbol AS ValueSymbol
        FROM CapabilityTypes
    ";

    public const string GetAll = $@"{SelectBase} WHERE Name LIKE @name ORDER BY Name";

    public const string GetByName = $@"{SelectBase} WHERE Name = @name";

    public const string Insert = @"
        INSERT INTO CapabilityTypes (Name, ActuatorMode, DataType, DynamicComputedValue, value_symbol)
        VALUES (@Name, @ActuatorMode, @DataType, @ComputedValue, @ValueSymbol);
        SELECT LAST_INSERT_ID() AS NewId;
    ";

    public const string InsertIcon = @"
        INSERT INTO CapabilityTypeIcons (CapabilityTypeId, name, active_color, inactive_color)
        VALUES (@CapabilityTypeId, @Name, @ActiveColor, @InactiveColor);
    ";

    public const string SelectIconsByTypeIds = @"
        SELECT 
            CapabilityTypeId,
            name AS Name,
            active_color AS ActiveColor,
            inactive_color AS InactiveColor
        FROM CapabilityTypeIcons
        WHERE CapabilityTypeId IN @ids
    ";

    public const string SelectIconsByTypeId = @"
        SELECT 
            CapabilityTypeId,
            name AS Name,
            active_color AS ActiveColor,
            inactive_color AS InactiveColor
        FROM CapabilityTypeIcons
        WHERE CapabilityTypeId = @id
    ";

    public const string UpdatePartialById = @"
        UPDATE CapabilityTypes
        SET
            Name = COALESCE(@Name, Name),
            ActuatorMode = COALESCE(@ActuatorMode, ActuatorMode),
            DataType = COALESCE(@DataType, DataType),
            DynamicComputedValue = COALESCE(@ComputedValue, DynamicComputedValue),
            value_symbol = COALESCE(@ValueSymbol, value_symbol)
        WHERE Id = @id;
    ";

    public const string DeleteIconsByTypeId = @"DELETE FROM CapabilityTypeIcons WHERE CapabilityTypeId = @id;";

    public const string DeleteTypeById = @"DELETE FROM CapabilityTypes WHERE Id = @id;";
}
