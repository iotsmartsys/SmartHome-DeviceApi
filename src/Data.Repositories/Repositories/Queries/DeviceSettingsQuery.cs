internal class DeviceSettingsQuery
{
    public const string InsertOrUpdateDeviceSettingsByDeviceId = @"
        INSERT INTO DeviceSettings (DeviceId, Name, Value, Description)
        VALUES (
            (SELECT Id FROM Devices WHERE DeviceId = @DeviceId),
            @Name,
            @Value,
            @Description
        ) AS new
        ON DUPLICATE KEY UPDATE
            Value = new.Value,
            Description = CASE
                WHEN new.Description IS NOT NULL AND TRIM(new.Description) <> '' THEN new.Description
                ELSE DeviceSettings.Description
            END;
        ";

    public const string GetDeviceSettingsByDeviceId = @"
        SELECT
            Name,
            Description,
            Value,
            Type
        FROM v_DeviceEffectiveSettings
        WHERE DeviceKey = @DeviceId;
        ";
}
