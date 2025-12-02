namespace Core.Defaults;

public record struct DateTimeISO8601
{
    public DateTimeISO8601(string value) => Value = value;
    public DateTimeISO8601() => Value = DateTime.Now.AsString();
    public string Value { get; init; }
    public static implicit operator DateTimeISO8601(DateTime dateTime) => new(dateTime.AsString());
    public static implicit operator DateTime(DateTimeISO8601 dateTime) => dateTime.Value.ToDateTime() ?? DateTime.Now;
    public static implicit operator DateTime?(DateTimeISO8601 dateTime) => dateTime.Value.ToDateTime();
    public static implicit operator string(DateTimeISO8601 dateTime) => dateTime.Value;
    public static implicit operator DateTimeISO8601(string dateTime) => new(dateTime);
}

public static class Helpers
{
public static bool IsIPAddress(this string input)
    {
        return System.Net.IPAddress.TryParse(input, out _);
    }
}