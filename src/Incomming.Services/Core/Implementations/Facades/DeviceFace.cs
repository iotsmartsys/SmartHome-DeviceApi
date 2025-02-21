using System.Net;
using System.Text.Json;
using Incomming.Service.Core.Contracts.Facades;
using Microsoft.Extensions.Options;

namespace Incomming.Service.Core.Implementations.Facades;

internal class DeviceFace(IHttpClientFactory httpClientFactory, IOptions<ClientDeviceApiSettings> apiSettings) : IDeviceFacade
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(apiSettings.GetName());

    public async Task<Device?> GetDeviceAsync(string device_id)
    {
        var response = await _httpClient.GetAsync(apiSettings.GetDeviceEndpoint() + device_id);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Device>(content);
    }
}