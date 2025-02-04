
namespace Core.Entities;

public struct DetectionDataType
{
    public static string Parse(string value) => value switch
    {
        "true" => DetectionDataType.Detected,
        "false" => DetectionDataType.Undetected,
        _ => value
    };
    public static readonly string Detected = "detected";
    public static readonly string Undetected = "undetected";
}
