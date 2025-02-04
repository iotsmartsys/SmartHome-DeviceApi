
namespace Core.Entities;

public struct PressDataType
{
    public static string Parse(string value) => value switch
    {
        "true" => PressDataType.Pressed,
        "false" => PressDataType.Released,
        _ => value
    };
    public static readonly string Pressed = "pressed";
    public static readonly string Released = "released";
}