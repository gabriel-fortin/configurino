using System.Diagnostics.CodeAnalysis;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// What a node is called among its siblings — one segment of a path, never a whole path.
/// </summary>
/// <remarks>
/// Names are canonicalised to lower case on the way in, so two names that differ only by case are the
/// same name. Doing it here rather than through a case-insensitive comparer keeps the record's
/// generated equality correct, which is what everything else relies on.
/// </remarks>
public sealed record NodeName
{
    public const int MaxLength = 64;

    public string Value { get; }

    private NodeName(string value) => Value = value;

    public static NodeName Parse(string? value)
    {
        if (!TryParse(value, out var name, out var reason))
        {
            throw new InvalidNodeNameException(value, reason);
        }

        return name;
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out NodeName? name) =>
        TryParse(value, out name, out _);

    private static bool TryParse(string? value, [NotNullWhen(true)] out NodeName? name, out string reason)
    {
        name = null;

        if (string.IsNullOrEmpty(value))
        {
            reason = "it is empty";
            return false;
        }

        if (value.Length > MaxLength)
        {
            reason = $"it is longer than {MaxLength} characters";
            return false;
        }

        var canonical = value.ToLowerInvariant();

        if (!IsLetterOrDigit(canonical[0]))
        {
            reason = "it must start with a letter or a digit";
            return false;
        }

        foreach (var character in canonical)
        {
            if (!IsLetterOrDigit(character) && character is not ('_' or '-'))
            {
                reason = $"'{character}' is not allowed; use letters, digits, '_' or '-'";
                return false;
            }
        }

        name = new NodeName(canonical);
        reason = string.Empty;
        return true;
    }

    private static bool IsLetterOrDigit(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9';

    public override string ToString() => Value;
}
