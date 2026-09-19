namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>A node name did not satisfy the naming rules.</summary>
public sealed class InvalidNodeNameException : DomainException
{
    public InvalidNodeNameException(string? attempted, string reason)
        : base($"'{attempted}' is not a valid node name: {reason}.")
    {
        Attempted = attempted;
        Reason = reason;
    }

    public string? Attempted { get; }

    public string Reason { get; }
}
