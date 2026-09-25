using Api.IntegrationTests;

Options options;
try { options = Options.Parse(args); }
catch (ArgumentException exception) { Console.Error.WriteLine(exception.Message); return 2; }

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };

ManifestStore store;
try { store = new ManifestStore(options); }
catch (Exception exception) when (exception is ArgumentException or IOException or System.Text.Json.JsonException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
using (store)
{
    DashboardApiClient api;
    try { api = new DashboardApiClient(Options.ValidateUrl(store.Manifest.BaseUrl), options.TimeoutSeconds, options.Command == "cleanup", cancellation.Token); }
    catch (Exception exception) when (exception is ArgumentException or FormatException)
    {
        Console.Error.WriteLine(exception.Message);
        return 2;
    }
    using var ownedClient = api;
    Console.WriteLine($"{options.Command}: runId={store.Manifest.RunId}; manifesto={options.StatePath}");
    try
    {
        var suite = new DashboardIntegrationSuite(api, store);
        if (options.Command == "run") await suite.RunAsync(options.CapabilityId);
        else await suite.CleanupAsync();
        return 0;
    }
    catch (Exception exception)
    {
        store.Manifest.Status = options.Command == "cleanup" ? "Cleanup incomplete" : "Failed";
        try { store.Save(); }
        catch (Exception saveError) { Console.Error.WriteLine($"Falha ao salvar manifesto: {saveError.Message}"); }
        Console.Error.WriteLine(exception is OperationCanceledException ? "Execução interrompida; recursos preservados para cleanup." : exception.Message);
        Console.Error.WriteLine($"""Limpeza independente: cleanup --state "{options.StatePath}" """);
        return 1;
    }
}
