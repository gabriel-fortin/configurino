namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// A registration declared no keys at all. Applying it would mark a service's entire schema obsolete,
/// which is almost always a client bug rather than an intent, so the domain refuses it.
/// </summary>
public sealed class EmptyRegistrationException : DomainException
{
    public EmptyRegistrationException()
        : base("A registration must declare at least one key; an empty declaration would obsolete the whole schema.")
    {
    }
}
