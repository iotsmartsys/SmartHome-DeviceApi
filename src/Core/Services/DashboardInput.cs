using System.Text.Json.Nodes;
using Core.Exceptions;

namespace Core.Services;

internal static class DashboardInput
{
    internal static DashboardException Invalid(string code, string field) => new(code, 400, $"Campo inválido: {field}.", field);
    internal static void Fields(JsonObject input, IEnumerable<string> allowed, string code = "INVALID_REQUEST", string prefix = "")
    {
        foreach (var key in input.Select(p => p.Key))
            if (!allowed.Contains(key, StringComparer.Ordinal)) throw Invalid(code, prefix + key);
    }
    internal static string? Text(JsonNode? node, string field, string code, int max, bool required = false, bool trim = true)
    {
        if (node is null)
        {
            if (required) throw Invalid(code, field);
            return null;
        }
        if (node is not JsonValue value || !value.TryGetValue<string>(out var text)) throw Invalid(code, field);
        if (trim) text = text.Trim();
        if ((required || trim) && text.Length == 0 || text.Length > max) throw Invalid(code, field);
        return text;
    }
    internal static int Integer(JsonNode? node, string field, string code, int min, int max)
    {
        if (node is not JsonValue value || !value.TryGetValue<int>(out var number) || number < min || number > max)
            throw Invalid(code, field);
        return number;
    }
    internal static bool Boolean(JsonNode? node, string field, string code)
    {
        if (node is not JsonValue value || !value.TryGetValue<bool>(out var flag)) throw Invalid(code, field);
        return flag;
    }
    internal static double Number(JsonNode? node, string field)
    {
        if (node is not JsonValue value || !value.TryGetValue<double>(out var number) || !double.IsFinite(number))
            throw Invalid("INVALID_WIDGET_CONFIG", field);
        return number;
    }
    internal static JsonObject Object(JsonNode? node, string field, string code) =>
        node as JsonObject ?? throw Invalid(code, field);
}
