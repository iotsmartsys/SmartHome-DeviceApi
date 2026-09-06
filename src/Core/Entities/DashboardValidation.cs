using Core.Exceptions;

namespace Core.Entities;

internal static class DashboardValidation
{
    internal static DashboardExceptionDomain Invalid(string code, string field) => new(code, $"Campo inválido: {field}.", field);
    internal static string? Text(string? value, string field, string code, int max, bool required = false, bool trim = true)
    {
        if (value is null)
        {
            if (required) throw Invalid(code, field);
            return null;
        }
        if (trim) value = value.Trim();
        if (((required || trim) && value.Length == 0) || value.Length > max) throw Invalid(code, field);
        return value;
    }
    internal static int Number(int value, string field, string code, int min, int max)
    {
        if (value < min || value > max) throw Invalid(code, field);
        return value;
    }
    internal static DateTime UtcNow()
    {
        var now = DateTime.UtcNow;
        return new DateTime(now.Ticks - now.Ticks % 10, DateTimeKind.Utc);
    }
}
