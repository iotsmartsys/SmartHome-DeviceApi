using Newtonsoft.Json;

namespace Api.Models;

[JsonConverter(typeof(DashboardRequestConverter))]
public sealed class DashboardPositionRequest
{
    private int? _x;
    private bool xSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? x { get => _x; set { _x = value; xSpecified = true; } }

    private int? _y;
    private bool ySpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? y { get => _y; set { _y = value; ySpecified = true; } }

    private int? _width;
    private bool widthSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? width { get => _width; set { _width = value; widthSpecified = true; } }

    private int? _height;
    private bool heightSpecified;
    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public int? height { get => _height; set { _height = value; heightSpecified = true; } }

    public Core.Contracts.Services.DashboardPositionRequest ToEntity() => new()
    {
        X = x, XSpecified = xSpecified,
        Y = y, YSpecified = ySpecified,
        Width = width, WidthSpecified = widthSpecified,
        Height = height, HeightSpecified = heightSpecified,
    };
}

public sealed record DashboardPosition(int x, int y, int width, int height)
{
    public static implicit operator DashboardPosition(Core.Entities.DashboardWidget widget) =>
        new(widget.X, widget.Y, widget.Width, widget.Height);
}
