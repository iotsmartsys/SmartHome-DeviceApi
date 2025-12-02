using System.Net;

namespace Api.Models;

public static class DataParserHelper
{
    public static Dictionary<string, object> ToDictionary(this IEnumerable<Core.Entities.Settings> settings, IEnumerable<string> prefixes)
    {
        var root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        prefixes = prefixes.Any() ? prefixes : ["mqtt"];
        HashSet<string> hierarchicalPrefixes = new(prefixes, StringComparer.OrdinalIgnoreCase);

        foreach (var setting in settings)
        {
            if (string.IsNullOrWhiteSpace(setting.Name))
                continue;

            var parts = setting.Name
                .Split('_', StringSplitOptions.RemoveEmptyEntries);

            var value = Parse(setting.Value);

            // Se tiver underscore e o primeiro segmento estiver na lista de prefixos hierárquicos
            if (parts.Length > 1 && hierarchicalPrefixes.Contains(parts[0]))
            {
                var current = root;

                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    var isLast = i == parts.Length - 1;

                    if (isLast)
                    {
                        // Último nível recebe o valor
                        current[part] = value;
                    }
                    else
                    {
                        // Desce/cria o próximo dicionário
                        if (!current.TryGetValue(part, out var existing) ||
                            existing is not Dictionary<string, object> childDict)
                        {
                            childDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            current[part] = childDict;
                        }

                        current = childDict;
                    }
                }
            }
            else
            {
                // Comportamento antigo: chave plana
                root[setting.Name] = value;
            }
        }

        return root;
    }
    static object Parse(string value)
    {
        if (IPAddress.TryParse(value, out _) && value.Contains('.'))
            return value;
        if (int.TryParse(value, out int intValue))
            return intValue;
        if (bool.TryParse(value, out bool boolValue))
            return boolValue;
        if (double.TryParse(value, out double doubleValue))
            return doubleValue;
        if (DateTime.TryParse(value, out DateTime dateTimeValue))
            return dateTimeValue;

        return value;
    }

}
