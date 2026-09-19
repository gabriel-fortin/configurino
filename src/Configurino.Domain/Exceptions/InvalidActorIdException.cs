namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>An actor id was empty, blank or too long to identify whoever performed an operation.</summary>
public sealed class InvalidActorIdException : DomainException
{
    public InvalidActorIdException(string? attempted, string reason)
        : base($"'{attempted}' is not a valid actor id: {reason}.")
    {
        Attempted = attempted;
        Reason = reason;
    }

    public string? Attempted { get; }

    public string Reason { get; }
}
