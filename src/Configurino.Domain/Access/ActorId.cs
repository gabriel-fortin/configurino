using System.Diagnostics.CodeAnalysis;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.Access;

/// <summary>
/// Whoever performed an operation — a person or a client application's service principal.
/// </summary>
/// <remarks>
/// Deliberately opaque: the domain records who acted without knowing anything about identity or what
/// they are allowed to do. When authorisation arrives it hooks in here, and no operation signature has
/// to change to accommodate it.
/// </remarks>
public sealed record ActorId
{
    public const int MaxLength = 200;

    private ActorId(string value) => Value = value;

    public string Value { get; }

    public static ActorId Parse(string? value)
    {
        if (!TryParse(value, out var actor, out var reason))
        {
            throw new InvalidActorIdException(value, reason);
        }

        return actor;
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out ActorId? actor) =>
        TryParse(value, out actor, out _);

    private static bool TryParse(string? value, [NotNullWhen(true)] out ActorId? actor, out string reason)
    {
        actor = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            reason = "it is empty";
            return false;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
        {
            reason = $"it is longer than {MaxLength} characters";
            return false;
        }

        actor = new ActorId(trimmed);
        reason = string.Empty;
        return true;
    }

    public override string ToString() => Value;
}
