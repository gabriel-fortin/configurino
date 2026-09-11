namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// Signals that an operation would violate a rule of the domain model.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}