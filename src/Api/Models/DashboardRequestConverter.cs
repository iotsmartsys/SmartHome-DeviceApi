using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Api.Models;

// Scoped to Dashboard request contracts. Preserve JSON types and field presence before binding.
public sealed class DashboardRequestConverter : JsonConverter
{
    public override bool CanWrite => false;
    public override bool CanConvert(Type objectType) =>
        typeof(DashboardWriteRequest).IsAssignableFrom(objectType) ||
        typeof(DashboardWidgetWriteRequest).IsAssignableFrom(objectType) ||
        objectType == typeof(DashboardPositionRequest) || objectType == typeof(DashboardWidgetConfig);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var prefix = objectType == typeof(DashboardWidgetConfig) ? "config"
            : objectType == typeof(DashboardPositionRequest) ? "position" : "request";
        if (reader.TokenType == JsonToken.Null && prefix != "request") return null;
        if (reader.TokenType != JsonToken.StartObject) throw Invalid(prefix);
        // Dates are source strings here; MVC must not convert an ISO-looking title into a Date token.
        reader.DateParseHandling = DateParseHandling.None;
        JObject body;
        try
        {
            body = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
        }
        catch (JsonReaderException) { throw Invalid(prefix); }
        var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(objectType);
        foreach (var member in body.Properties())
        {
            var field = prefix == "request" ? member.Name : prefix + "." + member.Name;
            var property = contract.Properties.FirstOrDefault(property => !property.Ignored && property.Writable && property.PropertyName == member.Name);
            if (property?.PropertyType is null) throw Invalid(field, prefix);
            if (member.Value.Type == JTokenType.Null) continue;
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var valid = type == typeof(string) ? member.Value.Type == JTokenType.String
                : type == typeof(bool) ? member.Value.Type == JTokenType.Boolean
                : type == typeof(int) ? member.Value.Type == JTokenType.Integer &&
                    int.TryParse(member.Value.ToString(Formatting.None), System.Globalization.NumberStyles.AllowLeadingSign,
                        System.Globalization.CultureInfo.InvariantCulture, out _)
                : type == typeof(double) ? member.Value.Type is JTokenType.Integer or JTokenType.Float &&
                    double.TryParse(member.Value.ToString(Formatting.None), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var number) && double.IsFinite(number)
                : member.Value.Type == JTokenType.Object;
            if (!valid) throw Invalid(field, prefix);
        }
        var model = Activator.CreateInstance(objectType)!;
        using var objectReader = body.CreateReader();
        objectReader.DateParseHandling = DateParseHandling.None;
        serializer.Populate(objectReader, model);
        return model;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();

    private static DashboardJsonException Invalid(string field, string? prefix = null) =>
        new(prefix == "config" || field == "config" ? "INVALID_WIDGET_CONFIG"
            : prefix == "position" || field == "position" ? "INVALID_WIDGET_POSITION" : "INVALID_REQUEST", field);
}

public sealed class DashboardJsonException(string code, string field) : JsonSerializationException("Campo JSON inválido.")
{
    public string Code { get; } = code;
    public string Field { get; } = field;
}
