
using System.Globalization;

namespace Core.Defaults;

public static class DateTimeExtensions
{
    public static string AsString(this DateTime dateTime, string format = DateTimeFormat.ISO8601) => dateTime.ToString(format);
    public static DateTime? ToDateTime(this string? dateTime, string format = DateTimeFormat.ISO8601)
    {
        if (dateTime is null)
            return null;

        if (DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            return parsedDate;

        return null;
    }
}
