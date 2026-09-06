namespace Core.Exceptions;

public class DashboardExceptionDomain(string code, string message, string? field = null) : Exception(message)
{
    public string Code { get; } = code;
    public string? Field { get; } = field;
}
