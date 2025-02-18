using Microsoft.Extensions.Options;

namespace Incomming.Service.Core.Implementations.Facades;

internal static class ApiClientExtensions
{
    public static string GetName(this IOptions<ClientDeviceApiSettings> settings) => settings.Value.Name;
    public static string GetBaseUrl(this IOptions<ClientDeviceApiSettings> settings) => settings.Value.BaseUrl;
    public static string GetDeviceEndpoint(this IOptions<ClientDeviceApiSettings> settings) => settings.Value.DeviceUri;
}