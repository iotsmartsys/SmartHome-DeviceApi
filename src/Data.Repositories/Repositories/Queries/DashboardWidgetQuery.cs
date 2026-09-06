namespace Data.Repositories;

internal static class DashboardWidgetQuery
{
    public const string GetAllWidgets = @"
    SELECT Id, DashboardId, CapabilityId, Title, WidgetType, DataMode, PositionX AS X, PositionY AS Y,
        Width, Height, ConfigJson, RefreshIntervalSeconds, DisplayOrder, CreatedAt, UpdatedAt
    FROM DashboardWidgets ORDER BY DisplayOrder, Id;
    ";

    public const string GetByDashboardId = @"
    SELECT Id, DashboardId, CapabilityId, Title, WidgetType, DataMode, PositionX AS X, PositionY AS Y,
        Width, Height, ConfigJson, RefreshIntervalSeconds, DisplayOrder, CreatedAt, UpdatedAt
    FROM DashboardWidgets WHERE DashboardId = @dashboardId ORDER BY DisplayOrder, Id;
    ";

    public const string Insert = @"
    INSERT INTO DashboardWidgets (DashboardId, CapabilityId, Title, WidgetType, DataMode, PositionX, PositionY,
        Width, Height, ConfigJson, RefreshIntervalSeconds, DisplayOrder, CreatedAt, UpdatedAt)
    VALUES (@DashboardId, @CapabilityId, @Title, @WidgetType, @DataMode, @X, @Y,
        @Width, @Height, @ConfigJson, @RefreshIntervalSeconds, @DisplayOrder, @CreatedAt, @UpdatedAt);
    SELECT LAST_INSERT_ID();
    ";

    public const string Update = @"
    UPDATE DashboardWidgets SET CapabilityId = @CapabilityId, Title = @Title, WidgetType = @WidgetType,
        DataMode = @DataMode, PositionX = @X, PositionY = @Y, Width = @Width, Height = @Height,
        ConfigJson = @ConfigJson, RefreshIntervalSeconds = @RefreshIntervalSeconds, DisplayOrder = @DisplayOrder, UpdatedAt = @UpdatedAt
    WHERE Id = @Id AND DashboardId = @DashboardId;
    ";

    public const string Delete = @"
    DELETE FROM DashboardWidgets WHERE Id = @widgetId AND DashboardId = @dashboardId;
    ";

}
