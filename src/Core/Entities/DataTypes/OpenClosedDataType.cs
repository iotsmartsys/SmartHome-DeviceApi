
namespace Core.Entities;

public struct OpenClosedDataType
{
    public static string Parse(string value) => value switch
    {
        "true" => OpenClosedDataType.Closed,
        "false" => OpenClosedDataType.Open,
        _ => value
    };
    public static readonly string Open = "open";
    public static readonly string Closed = "closed";
}
