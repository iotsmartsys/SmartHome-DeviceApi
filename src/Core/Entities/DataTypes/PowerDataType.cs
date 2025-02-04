
namespace Core.Entities;

public struct PowerDataType
{
    public static string Parse(string value) => value switch
    {
        "true" => PowerDataType.On,
        "false" => PowerDataType.Off,
        _ => value
    };
    public static readonly string On = "on";
    public static readonly string Off = "off";
}
