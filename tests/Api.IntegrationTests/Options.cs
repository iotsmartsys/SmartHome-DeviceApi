namespace Api.IntegrationTests;

internal sealed record Options(string Command, string StatePath, Uri? BaseUrl, int? CapabilityId, int TimeoutSeconds)
{
    internal const string Usage = """
        Dashboard HTTP integration suite (.NET 9)
        run     --base-url <http(s)://host[:port]/> --state <manifest.json> [--capability-id <id>] [--timeout-seconds <1..300>]
        cleanup --state <manifest.json> [--timeout-seconds <1..300>]
        Optional authentication: DASHBOARD_TEST_TOKEN environment variable.
        run never deletes resources. cleanup uses the URL and ownership markers from the manifest.
        """;

    internal static Options Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is not ("run" or "cleanup")) throw new ArgumentException(Usage);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 1; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length || args[index] is not ("--base-url" or "--state" or "--capability-id" or "--timeout-seconds") ||
                !values.TryAdd(args[index], args[index + 1])) throw new ArgumentException(Usage);
        }
        if (!values.TryGetValue("--state", out var state) || string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("Informe --state para preservar os dados da limpeza." + Environment.NewLine + Usage);
        var timeout = 30;
        if (values.TryGetValue("--timeout-seconds", out var rawTimeout) &&
            (!int.TryParse(rawTimeout, out timeout) || timeout is < 1 or > 300)) throw new ArgumentException("Timeout deve ser 1..300.");
        Uri? baseUrl = null;
        int? capabilityId = null;
        if (args[0] == "run")
        {
            if (!values.TryGetValue("--base-url", out var rawUrl)) throw new ArgumentException("Informe --base-url." + Environment.NewLine + Usage);
            baseUrl = ValidateUrl(rawUrl);
            if (values.TryGetValue("--capability-id", out var rawId))
            {
                if (!int.TryParse(rawId, out var id) || id <= 0) throw new ArgumentException("CapabilityId deve ser Int32 positivo.");
                capabilityId = id;
            }
        }
        else if (values.ContainsKey("--base-url") || values.ContainsKey("--capability-id"))
            throw new ArgumentException("cleanup utiliza exclusivamente o destino e os recursos registrados no manifesto.");
        return new(args[0], Path.GetFullPath(state), baseUrl, capabilityId, timeout);
    }

    internal static Uri ValidateUrl(string text)
    {
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") ||
            uri.UserInfo.Length != 0 || uri.Query.Length != 0 || uri.Fragment.Length != 0)
            throw new ArgumentException("URL deve ser HTTP(S), sem credenciais, query ou fragmento.");
        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }
}
