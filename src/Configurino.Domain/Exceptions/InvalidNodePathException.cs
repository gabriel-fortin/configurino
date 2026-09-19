namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>A node path could not be parsed.</summary>
public sealed class InvalidNodePathException : DomainException
{
    public InvalidNodePathException(string? attempted, string reason)
        : base($"'{attempted}' is not a valid node path: {reason}.")
    {
        Attempted = attempted;
        Reason = reason;
    }

    public string? Attempted { get; }

    public string Reason { get; }
}
