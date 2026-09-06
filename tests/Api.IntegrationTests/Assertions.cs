using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.IntegrationTests;

internal sealed class TestFailureException(string message) : Exception(message);

internal static class Check
{
    internal static void That(bool condition, string message)
    {
        if (!condition) throw new TestFailureException(message);
    }

    internal static void Equal<T>(T expected, T actual, string message) =>
        That(EqualityComparer<T>.Default.Equals(expected, actual), message);

    internal static void JsonEqual<T>(T expected, T actual, string message) =>
        That(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(expected, DashboardApiClient.JsonOptions),
            JsonSerializer.SerializeToNode(actual, DashboardApiClient.JsonOptions)), message);

    internal static DateTimeOffset Utc(string text)
    {
        That(text.EndsWith('Z') && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out _), "Timestamp deve ser RFC 3339 UTC com Z.");
        return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
    }

    internal static void Reading(string status, string? dataType, JsonElement value)
    {
        That(status is "ok" or "no_data" or "stale" or "offline" or "invalid_value" or "capability_missing" or "error", "Status de dados desconhecido.");
        if (status is not ("ok" or "stale"))
        {
            Equal(JsonValueKind.Null, value.ValueKind, "Ausência/falha deve emitir value=null, nunca zero/false/estado fabricado.");
            return;
        }
        var valid = dataType switch
        {
            "numeric" => value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) && double.IsFinite(number),
            "logical" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "state" => value.ValueKind == JsonValueKind.String && value.GetString() is "on" or "off" or "open" or "closed" or "pressed" or "released",
            "text" or "event" => value.ValueKind == JsonValueKind.String,
            _ => false
        };
        That(valid, "Valor de leitura incompatível com dataType/status.");
    }
}
