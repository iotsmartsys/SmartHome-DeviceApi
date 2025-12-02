public enum SettingsKeyTypes
{
    prefix_auto_format_properies_json,
    mqtt_primary_broker,
    mqtt_primary_broker_port,
    mqtt_primary_broker_user,
    mqtt_primary_broker_password
}

public static class SettingsKeyTypesExtensions
{
    public static bool Is(this SettingsKeyTypes _, string value) => _ switch
    {
        SettingsKeyTypes.prefix_auto_format_properies_json => value == "prefix_auto_format_properies_json",
        SettingsKeyTypes.mqtt_primary_broker => value == "mqtt_primary_host",
        SettingsKeyTypes.mqtt_primary_broker_port => value == "mqtt_primary_port",
        SettingsKeyTypes.mqtt_primary_broker_user => value == "mqtt_primary_user",
        SettingsKeyTypes.mqtt_primary_broker_password => value == "mqtt_primary_password",
        _ => throw new NotImplementedException()
    };

}