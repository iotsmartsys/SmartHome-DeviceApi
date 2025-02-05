public class NotFoundExceptionDomain(string message) : DomainException(message);
public class DomainException(string message) : Exception(message);
public class ArgumentDomainException(string message, params string[] paramName) : DomainException(message)
{
    public string[] ParamName { get; } = paramName;
}