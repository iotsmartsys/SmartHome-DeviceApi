
namespace Core.Entities;

public struct OpenClosedDataType
{
    public static string Parse(string value) => value switch
    {
        "true" => OpenClosedDataType.Open,
        "false" => OpenClosedDataType.Closed,
        _ => value
    };
    public static readonly string Open = "open";
    public static readonly string Closed = "closed";
}
