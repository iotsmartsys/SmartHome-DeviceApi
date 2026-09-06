using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Api.IntegrationTests;

internal sealed class DashboardApiClient : IDisposable
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
        WriteIndented = true
    };
    private readonly HttpClient client;
    private readonly bool cleanup;
    private readonly CancellationToken cancellationToken;

    internal DashboardApiClient(Uri baseUrl, int timeoutSeconds, bool cleanup, CancellationToken cancellationToken)
    {
        this.cleanup = cleanup;
        this.cancellationToken = cancellationToken;
        client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        { BaseAddress = baseUrl, Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        var token = Environment.GetEnvironmentVariable("DASHBOARD_TEST_TOKEN");
        if (!string.IsNullOrWhiteSpace(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    internal async Task<T> GetAsync<T>(string path) => (await SendAsync<T>(HttpMethod.Get, path, null, 200)).Body;
    internal Task<ApiResponse<T>> PostAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Post, path, body, 201);
    internal async Task<T> PutAsync<T>(string path, object body) => (await SendAsync<T>(HttpMethod.Put, path, body, 200)).Body;

    internal async Task<DashboardResponse?> GetDashboardOrMissingAsync(long id)
    {
        using var response = await SendAsync(HttpMethod.Get, $"api/v1/dashboards/{id}", null);
        if ((int)response.StatusCode == 404)
        {
            await ReadErrorAsync(response, 404, "DASHBOARD_NOT_FOUND", null);
            return null;
        }
        return await ReadAsync<DashboardResponse>(response, 200);
    }

    internal async Task<CompatibleWidgetsResponse?> GetCapabilityOrMissingAsync(int id)
    {
        using var response = await SendAsync(HttpMethod.Get, $"api/v1/dashboard-capabilities/{id}/compatible-widgets", null);
        if ((int)response.StatusCode == 404)
        {
            await ReadErrorAsync(response, 404, "CAPABILITY_NOT_FOUND", null);
            return null;
        }
        return await ReadAsync<CompatibleWidgetsResponse>(response, 200);
    }

    internal async Task DeleteAsync(string path)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, null);
        ExpectStatus(response, 204);
        Check.That((await response.Content.ReadAsByteArrayAsync(cancellationToken)).Length == 0, "DELETE 204 deve retornar corpo vazio.");
    }

    internal async Task ErrorAsync(HttpMethod method, string path, object? body, int status, string code, string? field = null)
    {
        using var response = await SendAsync(method, path, body);
        await ReadErrorAsync(response, status, code, field);
    }

    internal async Task InvalidBodyAsync(string path, string body, string mediaType, int status, string code)
    {
        using var response = await SendAsync(HttpMethod.Put, path, new StringContent(body, Encoding.UTF8, mediaType));
        await ReadErrorAsync(response, status, code, null);
    }

    internal void Location(Uri? location, string path)
    {
        Check.That(location is not null, "POST deve retornar Location.");
        var actual = location!.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);
        Check.Equal(new Uri(client.BaseAddress!, path).AbsolutePath, actual.AbsolutePath, "Location não aponta para GET do dashboard.");
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string path, object? body, int status)
    {
        using var response = await SendAsync(method, path, body);
        return new(await ReadAsync<T>(response, status), response.Headers.Location);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body)
    {
        if (!cleanup && method == HttpMethod.Delete) throw new InvalidOperationException("run não permite DELETE.");
        if (cleanup && method != HttpMethod.Delete && method != HttpMethod.Get) throw new InvalidOperationException("cleanup permite somente GET/DELETE.");
        using var request = new HttpRequestMessage(method, path);
        if (body is HttpContent content) request.Content = content;
        else if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, body.GetType(), JsonOptions), Encoding.UTF8, "application/json");
        // No retries: a lost POST response is recovered by the fixture marker during cleanup.
        return await client.SendAsync(request, cancellationToken);
    }

    private static void ExpectStatus(HttpResponseMessage response, int expected) => Check.That((int)response.StatusCode == expected,
        $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.AbsolutePath}: HTTP esperado {expected}, recebido {(int)response.StatusCode}.");

    private async Task<T> ReadAsync<T>(HttpResponseMessage response, int status)
    {
        ExpectStatus(response, status);
        Check.Equal("application/json", response.Content.Headers.ContentType?.MediaType, "Resposta deve usar application/json.");
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(body, JsonOptions) ?? throw new TestFailureException("Resposta JSON null inesperada.");
    }

    private async Task ReadErrorAsync(HttpResponseMessage response, int status, string code, string? field)
    {
        var error = (await ReadAsync<ErrorResponse>(response, status)).Error;
        Check.Equal(code, error.Code, "Código de erro divergente.");
        Check.That(!string.IsNullOrWhiteSpace(error.Message), "Mensagem de erro ausente.");
        Check.Equal(JsonValueKind.Object, error.Details.ValueKind, "error.details deve ser objeto.");
        if (field is not null)
            Check.That(error.Details.TryGetProperty("field", out var actual) && actual.GetString() == field, "error.details.field divergente.");
    }

    public void Dispose() => client.Dispose();
}
