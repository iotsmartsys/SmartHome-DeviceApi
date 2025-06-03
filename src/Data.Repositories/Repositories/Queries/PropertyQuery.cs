namespace Data.Repositories;

internal static class PropertyQuery
{
    public const string GetIdDevice = @"SELECT Id FROM Devices WHERE DeviceId = @device_id;";
    public const string AddForDevice = @"
        INSERT INTO DeviceProperties(DeviceId, Name, Description, Value) 
        VALUES(@deviceId, @name, @description, @value);
        
        SELECT LAST_INSERT_ID() AS NewId;";
    public const string UpdateForDevice = @"UPDATE DeviceProperties SET Name = @name, Description = @description, Value = @value WHERE DeviceId = @deviceId AND Id = @id;";
    public const string RemoveForDevice = @"DELETE FROM DeviceProperties WHERE DeviceId = @deviceId AND Id = @id;";
    public const string GetAll = @"
        SELECT 
            dp.Id, 
            dp.Name, 
            dp.Description, 
            dp.Value 
        FROM DeviceProperties dp
        INNER JOIN Devices d ON dp.DeviceId = d.Id
        WHERE  
            d.DeviceId = @device_id ";

    public const string UpSert = @"
        INSERT INTO DeviceProperties (DeviceId, Name, Description, Value)
        VALUES ((SELECT Id FROM Devices WHERE deviceId = @device_id), @name, @description, @value)
        ON DUPLICATE KEY UPDATE
            Name = @name,
            Description = @description,
            Value = @value;
    ";
}