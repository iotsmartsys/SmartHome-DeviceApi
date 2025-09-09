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
            ValueSymbol AS ValueSymbol,
            ActiveValue
        FROM CapabilityTypes
    ";

    public const string GetAll = $@"{SelectBase} WHERE Name LIKE @name ORDER BY Name";

    public const string GetByName = $@"{SelectBase} WHERE Name = @name";

    public const string Insert = @"
        INSERT INTO CapabilityTypes (Name, ActuatorMode, DataType, DynamicComputedValue, ValueSymbol)
        VALUES (@Name, @ActuatorMode, @DataType, @ComputedValue, @ValueSymbol);
        SELECT LAST_INSERT_ID() AS NewId;
    ";

    public const string InsertIcon = @"
        INSERT INTO CapabilityTypeIcons (CapabilityTypeId, Name, PrimaryColor, SecondaryColor)
        VALUES (@CapabilityTypeId, @Name, @PrimaryColor, @SecondaryColor);
    ";

    public const string SelectIconsByTypeIds = @"
        SELECT 
            CapabilityTypeId,
            Name AS Name,
            PrimaryColor AS PrimaryColor,
            SecondaryColor AS SecondaryColor
        FROM CapabilityTypeIcons
        WHERE CapabilityTypeId IN @ids
    ";

    public const string SelectIconsByTypeId = @"
        SELECT 
            CapabilityTypeId,
            Name AS Name,
            PrimaryColor AS PrimaryColor,
            SecondaryColor AS SecondaryColor
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
            ValueSymbol = @ValueSymbol,
            ActiveValue = @ActiveValue
        WHERE Id = @id;
    ";

    public const string DeleteIconsByTypeId = @"DELETE FROM CapabilityTypeIcons WHERE CapabilityTypeId = @id;";

    public const string DeleteTypeById = @"DELETE FROM CapabilityTypes WHERE Id = @id;";
}
