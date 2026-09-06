namespace Data.Repositories;

internal static class DashboardCapabilityQuery
{
    public const string GetAllCapabilities = @"
    SELECT c.Id AS CapabilityId, c.Name AS CapabilityName, ct.Name AS CapabilityCode,
        ct.DataType AS SourceDataType, ct.ValueSymbol AS Unit,
        d.DeviceId, d.Name AS DeviceName, (d.Id IS NOT NULL) AS DeviceExists,
        COALESCE(d.Active, FALSE) AS DeviceActive, d.Status AS DeviceState,
        c.Active, c.Value, c.UpdatedAt
    FROM Capabilities c
        LEFT JOIN CapabilityTypes ct ON ct.Id = c.CapabilityTypeId
        LEFT JOIN Devices d ON d.Id = c.DeviceId
    ";
    public const string GetById = GetAllCapabilities + " WHERE c.Id = @id;";
    public const string GetByIdForUpdate = GetAllCapabilities + " WHERE c.Id = @id LOCK IN SHARE MODE;";
    public const string GetOrdered = GetAllCapabilities + " ORDER BY c.Id;";
    public const string DeviceExists = @"
    SELECT EXISTS(SELECT 1 FROM Devices WHERE BINARY DeviceId = BINARY @deviceId);
    ";
}
