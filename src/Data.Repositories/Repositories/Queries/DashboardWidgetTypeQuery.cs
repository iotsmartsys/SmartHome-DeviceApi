namespace Data.Repositories;

internal static class DashboardWidgetTypeQuery
{
    public const string GetAllWidgetTypes = @"
    SELECT Code, Name, Description, CompatibleDataTypes AS CompatibleDataTypesJson,
        DefaultDataMode, DefaultConfigJson, Enabled, Lifecycle FROM DashboardWidgetTypes;
    ";

}
