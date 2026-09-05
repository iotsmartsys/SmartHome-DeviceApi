namespace Core.Exceptions;

public sealed class DashboardException(string code, int statusCode, string message,
    string? field = null) : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;
    public string? Field { get; } = field;
}
