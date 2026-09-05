namespace Data.Repositories;

internal static class DashboardQuery
{
    public const string Dashboards = """
        SELECT id Id, name Name, description Description, layout_type LayoutType,
               is_default IsDefault, display_order DisplayOrder, created_at CreatedAt, updated_at UpdatedAt
        FROM dashboards
        """;
    public const string Widgets = """
        SELECT id Id, dashboard_id DashboardId, capability_id CapabilityId, title Title,
               widget_type WidgetType, data_mode DataMode, position_x X, position_y Y,
               width Width, height Height, config_json ConfigJson,
               refresh_interval_seconds RefreshIntervalSeconds, display_order DisplayOrder,
               created_at CreatedAt, updated_at UpdatedAt
        FROM dashboard_widgets
        """;
    public const string Types = """
        SELECT code Code, name Name, description Description,
               compatible_data_types CompatibleDataTypesJson, default_data_mode DefaultDataMode,
               default_config_json DefaultConfigJson, enabled Enabled, lifecycle Lifecycle
        FROM dashboard_widget_types
        """;
    public const string Sources = """
        SELECT c.Id CapabilityId, c.Name CapabilityName, ct.Name CapabilityCode,
               ct.DataType SourceDataType, ct.ValueSymbol Unit,
               d.DeviceId DeviceId, d.Name DeviceName, (d.Id IS NOT NULL) DeviceExists,
               COALESCE(d.Active, FALSE) DeviceActive, d.Status DeviceState,
               c.Active Active, c.Value Value, c.UpdatedAt UpdatedAt
        FROM Capabilities c
        LEFT JOIN CapabilityTypes ct ON ct.Id = c.CapabilityTypeId
        LEFT JOIN Devices d ON d.Id = c.DeviceId
        """;
}
