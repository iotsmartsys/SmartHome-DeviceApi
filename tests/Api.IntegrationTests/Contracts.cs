using System.Text.Json;

namespace Api.IntegrationTests;

// Independent wire contracts: the suite does not reference the API/Core assemblies.
internal sealed record DashboardResponse(long Id, string Name, string? Description, string LayoutType,
    bool IsDefault, int DisplayOrder, string CreatedAt, string? UpdatedAt, WidgetResponse[] Widgets);
internal sealed record DashboardSummary(long Id, string Name, string? Description, string LayoutType,
    bool IsDefault, int DisplayOrder, string CreatedAt, string? UpdatedAt, int WidgetCount);
internal sealed record ItemsResponse<T>(T[] Items);
internal sealed record Position(int X, int Y, int Width, int Height);
internal sealed record WidgetResponse(long Id, long DashboardId, string? Title, int CapabilityId, string? DeviceId,
    string? CapabilityCode, string? DataType, string WidgetType, string DataMode, Position Position,
    JsonElement Config, int? RefreshIntervalSeconds, int DisplayOrder, string CreatedAt, string? UpdatedAt);
internal sealed record WidgetTypeResponse(string Code, string Name, string? Description,
    string[] CompatibleDataTypes, string DefaultDataMode, JsonElement DefaultConfig, bool Enabled, string Lifecycle);
internal sealed record CapabilityResponse(string? DeviceId, string? DeviceName, int CapabilityId, string? CapabilityCode,
    string? CapabilityName, string? DataType, string? Unit, string? SemanticType, JsonElement CurrentValue,
    string? LastUpdatedAt, string Status, string[] CompatibleWidgets);
internal sealed record CompatibleWidgetsResponse(int CapabilityId, string? CapabilityCode, string? DataType,
    WidgetTypeResponse[] CompatibleWidgets);
internal sealed record DashboardDataResponse(long DashboardId, string Name, string LayoutType, string GeneratedAt,
    WidgetDataResponse[] Widgets);
internal sealed record WidgetDataResponse(long WidgetId, string? Title, string? DeviceId, int CapabilityId,
    string? CapabilityCode, string WidgetType, string? DataType, string DataMode, JsonElement Value, string? Unit,
    string? Label, string? Icon, string Status, string? LastUpdatedAt, Position Position, JsonElement Config,
    int DisplayOrder, int? RefreshIntervalSeconds);
internal sealed record ErrorResponse(ApiError Error);
internal sealed record ApiError(string Code, string Message, JsonElement Details);
internal sealed record ApiResponse<T>(T Body, Uri? Location);

// Separate payload shapes deliberately distinguish omitted members from explicit JSON null.
internal sealed record DashboardCreateRequest(string Name, string Description);
internal sealed record DashboardUpdateRequest(string Name, int DisplayOrder);
internal sealed record DashboardResetRequest(string? LayoutType = null, bool? IsDefault = null, int? DisplayOrder = null);
internal sealed record DashboardNameRequest(string? Name);
internal sealed record DashboardNumericNameRequest(int Name);
internal sealed record DashboardLayoutRequest(string LayoutType);
internal sealed record UnknownFieldRequest(string Unknown);
internal sealed record WidgetCreateRequest(int CapabilityId, string? DeviceId, string WidgetType, string Title);
internal sealed record WidgetUpdateRequest(string Title, Position Position, int RefreshIntervalSeconds, int DisplayOrder);
internal sealed record PositionHeightRequest(int? Height);
internal sealed record WidgetPositionRequest(PositionHeightRequest Position);
internal sealed record WidgetFullPositionRequest(Position Position);
internal sealed record WidgetTitleRequest(string Title);
internal sealed record WidgetRefreshRequest(int RefreshIntervalSeconds);
internal sealed record WidgetModeRequest(string DataMode);
internal sealed record UnitConfigRequest(string Unit);
internal sealed record GaugeConfigRequest(double Max, int Decimals);
internal sealed record GaugeMaxRequest(double Max);
internal sealed record DecimalsConfigRequest(int Decimals);
internal sealed record WidgetConfigRequest<T>(T Config);
internal sealed record WidgetResetRequest(string? Title = null, Position? Position = null,
    UnitConfigRequest? Config = null, int? RefreshIntervalSeconds = null, int? DisplayOrder = null);
internal sealed record EmptyRequest;
