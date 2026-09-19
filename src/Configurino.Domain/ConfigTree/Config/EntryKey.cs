using System.Diagnostics.CodeAnalysis;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.ConfigTree.Config;

/// <summary>
/// The name of a single configuration key within a config node, as declared by a client application.
/// </summary>
/// <remarks>
/// Canonicalised to lower case for the same reason node names are. '.' and ':' are allowed so that
/// client applications can keep the hierarchical key names they already use, such as
/// <c>db.pool.size</c>, without the domain reading any structure into them.
/// </remarks>
public sealed record EntryKey
{
    public const int MaxLength = 128;

    private EntryKey(string value) => Value = value;

    public string Value { get; }

    public static EntryKey Parse(string? value)
    {
        if (!TryParse(value, out var name, out var reason))
        {
            throw new InvalidConfigKeyNameException(value, reason);
        }

        return name;
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out EntryKey? name) =>
        TryParse(value, out name, out _);

    private static bool TryParse(string? value, [NotNullWhen(true)] out EntryKey? name, out string reason)
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
            if (!IsLetterOrDigit(character) && character is not ('.' or ':' or '_' or '-'))
            {
                reason = $"'{character}' is not allowed; use letters, digits, '.', ':', '_' or '-'";
                return false;
            }
        }

        name = new EntryKey(canonical);
        reason = string.Empty;
        return true;
    }

    private static bool IsLetterOrDigit(char character) =>
        character is >= 'a' and <= 'z' or >= '0' and <= '9';

    public override string ToString() => Value;
}
