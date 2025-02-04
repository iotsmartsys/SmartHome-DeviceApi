
namespace Core.Entities;

public record struct CapabilityDataType(string Value)
{
    public const string Float = "float";
    public const string Detection = "detection";
    public const string Boolean = "boolean";
    public const string OpenClosed = "open_closed";
    public const string Power = "power";
    public const string Null = "null";
    public const string Text = "text";
    public const string Time = "time";
    public const string Press = "press";

    public static implicit operator string(CapabilityDataType dataType) => dataType.Value;
    public static implicit operator CapabilityDataType(string value) => new CapabilityDataType(value);

    public string Convert(string value)
    {
        return Value switch
        {
            Float => float.Parse(value).ToString(),
            Detection => DetectionDataType.Parse(value),
            Boolean => bool.Parse(value).ToString(),
            OpenClosed => OpenClosedDataType.Parse(value),
            "power" => PowerDataType.Parse(value),
            "text" => value,
            "time" => DateTime.Parse(value).ToString(),
            "press" => PressDataType.Parse(value),
            _ => value
        };
    }

}
