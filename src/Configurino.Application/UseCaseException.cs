namespace org.g14.Configurino.Application;

/// <summary>
/// Base type for failures a use case raises on its own — as opposed to rule violations raised by the
/// domain, which travel as <c>DomainException</c>. A host can map the two families separately.
/// </summary>
public abstract class UseCaseException : Exception
{
    protected UseCaseException()
    {
    }

    protected UseCaseException(string message) : base(message)
    {
    }

    protected UseCaseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
