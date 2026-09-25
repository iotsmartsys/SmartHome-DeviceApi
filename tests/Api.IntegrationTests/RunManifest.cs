using System.Text.Json;

namespace Api.IntegrationTests;

internal sealed class RunManifest
{
    public int Version { get; set; } = 1;
    public required string RunId { get; set; }
    public required string BaseUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string Status { get; set; } = "Not Executed";
    public int? CapabilityId { get; set; }
    public List<DashboardFixture> Dashboards { get; set; } = [new() { Key = "primary" }, new() { Key = "secondary" }];
    public List<ScenarioResult> Results { get; set; } = [];
}

internal sealed class DashboardFixture
{
    public required string Key { get; set; }
    public long? Id { get; set; }
    public List<long> WidgetIds { get; set; } = [];
    internal string Name(string runId, bool updated = false) => $"IT Dashboard {runId} {Key}" + (updated ? " updated" : "");
    internal string Marker(string runId) => $"dashboard-api-integration:{runId}:{Key}";
    internal bool Owns(DashboardResponse dashboard, string runId) => dashboard.Description == Marker(runId);
}

internal sealed class ScenarioResult
{
    public required string Id { get; set; }
    public string Status { get; set; } = "Not Executed";
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Error { get; set; }
    public string? Note { get; set; }
}

internal sealed class ManifestStore : IDisposable
{
    private readonly string path;
    private readonly FileStream ownershipLock;
    internal RunManifest Manifest { get; }

    internal ManifestStore(Options options)
    {
        path = options.StatePath;
        if (options.Command == "cleanup" && !File.Exists(path)) throw new ArgumentException("Manifesto inexistente; nenhum recurso será excluído.");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try { ownershipLock = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException) { throw new ArgumentException("Manifesto em uso. Aguarde ou encerre a execução antes de limpar."); }
        try
        {
            if (options.Command == "run")
            {
                if (File.Exists(path)) throw new ArgumentException("Manifesto já existe. Use outro caminho para nova execução; preserve este para cleanup.");
                Manifest = new RunManifest { RunId = Guid.NewGuid().ToString("N"), BaseUrl = options.BaseUrl!.AbsoluteUri };
                foreach (var id in Enumerable.Range(1, 9)) Manifest.Results.Add(new ScenarioResult { Id = $"IT-{id:00}" });
                Save(); // Reserved names/markers are durable before any HTTP request, including POST.
            }
            else
            {
                Manifest = JsonSerializer.Deserialize<RunManifest>(File.ReadAllText(path), DashboardApiClient.JsonOptions)
                    ?? throw new ArgumentException("Manifesto inválido.");
                if (Manifest.Version != 1 || Manifest.CapabilityId is <= 0 || !Guid.TryParseExact(Manifest.RunId, "N", out _) ||
                    Manifest.Dashboards.Count != 2 || !Manifest.Dashboards.Select(item => item.Key).Order().SequenceEqual(new[] { "primary", "secondary" }) ||
                    Manifest.Dashboards.Any(item => item.Id is <= 0 || item.WidgetIds.Any(id => id <= 0)))
                    throw new ArgumentException("Manifesto inválido; não é seguro identificar seus recursos.");
                Options.ValidateUrl(Manifest.BaseUrl);
            }
        }
        catch { ownershipLock.Dispose(); throw; }
    }

    internal void Save()
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(Manifest, DashboardApiClient.JsonOptions);
        using (var output = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
        {
            output.Write(bytes);
            output.Flush(flushToDisk: true);
        }
        File.Move(path + ".tmp", path, overwrite: true);
    }

    public void Dispose() => ownershipLock.Dispose();
}
