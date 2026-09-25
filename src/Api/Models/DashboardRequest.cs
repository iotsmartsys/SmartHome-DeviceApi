using Newtonsoft.Json;

namespace Api.Models;

[JsonConverter(typeof(DashboardRequestConverter))]
public abstract class DashboardWriteRequest
{
    private string? _name;
    private bool nameSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? name { get => _name; set { _name = value; nameSpecified = true; } }

    private string? _description;
    private bool descriptionSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? description { get => _description; set { _description = value; descriptionSpecified = true; } }

    private string? _layoutType;
    private bool layoutTypeSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public string? layoutType { get => _layoutType; set { _layoutType = value; layoutTypeSpecified = true; } }

    private bool? _isDefault;
    private bool isDefaultSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public bool? isDefault { get => _isDefault; set { _isDefault = value; isDefaultSpecified = true; } }

    private int? _displayOrder;
    private bool displayOrderSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? displayOrder { get => _displayOrder; set { _displayOrder = value; displayOrderSpecified = true; } }

    public Core.Contracts.Services.DashboardRequest ToRequest() => new()
    {
        Name = name, NameSpecified = nameSpecified,
        Description = description, DescriptionSpecified = descriptionSpecified,
        LayoutType = layoutType, LayoutTypeSpecified = layoutTypeSpecified,
        IsDefault = isDefault, IsDefaultSpecified = isDefaultSpecified,
        DisplayOrder = displayOrder, DisplayOrderSpecified = displayOrderSpecified,
    };
}

public sealed class DashboardCreateRequest : DashboardWriteRequest, ISelfValidate
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Core.Exceptions.DashboardExceptionDomain("INVALID_DASHBOARD_NAME", "Name é obrigatório.", nameof(name));
    }
}

public sealed class DashboardUpdateRequest : DashboardWriteRequest;
