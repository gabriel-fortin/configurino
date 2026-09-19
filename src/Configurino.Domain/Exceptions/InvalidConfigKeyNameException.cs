namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>A config key name did not satisfy the naming rules.</summary>
public sealed class InvalidConfigKeyNameException : DomainException
{
    public InvalidConfigKeyNameException(string? attempted, string reason)
        : base($"'{attempted}' is not a valid config key name: {reason}.")
    {
        Attempted = attempted;
        Reason = reason;
    }

    public string? Attempted { get; }

    public string Reason { get; }
}
