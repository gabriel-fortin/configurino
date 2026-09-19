namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// Base type for every rule violation raised by the domain. Callers that want to translate domain
/// failures into transport-level responses can catch this single type.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
