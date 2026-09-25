using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.Entities;

public sealed record AirConditionerState(
    [property: JsonPropertyName("power")] string Power,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("temperature")] int Temperature)
{
    private static readonly AirConditionerState Defaults = new("off", "cool", 22);
    private static readonly HashSet<string> Modes = ["cool", "heat", "dry", "fan", "auto"];
    private static readonly IReadOnlyDictionary<string, Func<AirConditionerState, JsonElement, AirConditionerState>> Fields =
        new Dictionary<string, Func<AirConditionerState, JsonElement, AirConditionerState>>(StringComparer.Ordinal)
        {
            ["power"] = (state, value) => state with { Power = ReadPower(value) },
            ["mode"] = (state, value) => state with { Mode = ReadMode(value) },
            ["temperature"] = (state, value) => state with { Temperature = ReadTemperature(value) }
        };

    public static bool IsDataType(string? dataType) =>
        string.Equals(dataType, CapabilityDataType.AirConditioner, StringComparison.Ordinal);

    public static string Initialize(string? input) =>
        string.IsNullOrWhiteSpace(input) ? Serialize(Defaults) : Serialize(Parse(Defaults, input, true));

    public static string Normalize(string? stored) => Serialize(ReadStored(stored));

    public static string Apply(string? stored, string? input)
    {
        var state = ReadStored(stored);
        if (string.IsNullOrWhiteSpace(input)) throw InvalidInput();
        return Serialize(Parse(state, input, true));
    }

    private static AirConditionerState ReadStored(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return Defaults;
        try
        {
            // A stored object is a snapshot, not a new command to turn the device on.
            return Parse(Defaults, stored, false);
        }
        catch (ArgumentDomainException)
        {
            throw new AirConditionerStateConflictException();
        }
    }

    private static AirConditionerState Parse(AirConditionerState state, string input, bool isCommand)
    {
        var token = input.Trim();
        if (token is "on" or "off") return state with { Power = token };
        if (Modes.Contains(token)) return state with { Mode = token, Power = "on" };
        if (token.Length == 2 && int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var temperature)
            && temperature is >= 16 and <= 32)
            return state with { Temperature = temperature };

        try
        {
            using var document = JsonDocument.Parse(token);
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw InvalidInput();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in document.RootElement.EnumerateObject())
            {
                if (!seen.Add(field.Name) || !Fields.TryGetValue(field.Name, out var apply)) throw InvalidInput();
                state = apply(state, field.Value);
            }
            if (isCommand && seen.Contains("mode") && !seen.Contains("power"))
                state = state with { Power = "on" };
            return state;
        }
        catch (JsonException)
        {
            throw InvalidInput();
        }
    }

    private static string ReadPower(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return Defaults.Power;
        if (value.ValueKind == JsonValueKind.String && value.GetString() is "on" or "off") return value.GetString()!;
        throw InvalidInput();
    }

    private static string ReadMode(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return Defaults.Mode;
        if (value.ValueKind == JsonValueKind.String && Modes.Contains(value.GetString()!)) return value.GetString()!;
        throw InvalidInput();
    }

    private static int ReadTemperature(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Null) return Defaults.Temperature;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number is >= 16 and <= 32)
            return number;
        throw InvalidInput();
    }

    private static string Serialize(AirConditionerState state) => JsonSerializer.Serialize(state);
    private static ArgumentDomainException InvalidInput() => new("Invalid air conditioner state or command.", "value");
}
