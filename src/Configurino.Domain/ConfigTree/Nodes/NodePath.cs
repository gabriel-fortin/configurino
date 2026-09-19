using System.Diagnostics.CodeAnalysis;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.ConfigTree.Nodes;

/// <summary>
/// How callers address a node from outside: the names from the root down, separated by '/'.
/// </summary>
/// <remarks>
/// A path is an address, not an identity — <see cref="NodeId"/> is the identity. Resolving a path to a
/// node is the job of <c>INodeDirectory</c>; nothing in the domain stores one.
/// </remarks>
public sealed record NodePath
{
    private const char Separator = '/';
    public const int MaxDepth = 32;

    private readonly NodeName[] _segments;

    public string Value { get; }

    public IReadOnlyList<NodeName> Segments => _segments;

    public bool IsRoot => _segments.Length == 0;

    public int Depth => _segments.Length;

    /// <summary>The last segment, or <c>null</c> at the root, which has no name of its own.</summary>
    public NodeName? Name => IsRoot ? null : _segments[^1];

    /// <summary>The path this one sits under, or <c>null</c> at the root.</summary>
    public NodePath? Parent => IsRoot ? null : new NodePath(_segments[..^1]);

    private NodePath(NodeName[] segments)
    {
        _segments = segments;
        Value = segments.Length == 0
            ? Separator.ToString()
            : Separator + string.Join(Separator, segments.Select(segment => segment.Value));
    }

    /// <summary>The path of the grouping node that exists by default.</summary>
    public static NodePath Root { get; } = new([]);

    public NodePath Append(NodeName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Depth == MaxDepth)
        {
            throw new InvalidNodePathException($"{Value}{Separator}{name}", $"it is deeper than {MaxDepth} levels");
        }

        return new NodePath([.. _segments, name]);
    }

    public static NodePath Parse(string? value)
    {
        if (!TryParse(value, out var path, out var reason))
        {
            throw new InvalidNodePathException(value, reason);
        }

        return path;
    }

    public static bool TryParse(string? value, [NotNullWhen(true)] out NodePath? path) =>
        TryParse(value, out path, out _);

    private static bool TryParse(string? value, [NotNullWhen(true)] out NodePath? path, out string reason)
    {
        path = null;

        if (string.IsNullOrEmpty(value))
        {
            reason = "it is empty";
            return false;
        }

        if (value[0] != Separator)
        {
            reason = $"it must start with '{Separator}'";
            return false;
        }

        if (value.Length == 1)
        {
            path = Root;
            reason = string.Empty;
            return true;
        }

        var parts = value[1..].Split(Separator);

        if (parts.Length > MaxDepth)
        {
            reason = $"it is deeper than {MaxDepth} levels";
            return false;
        }

        var parsed = new NodeName[parts.Length];

        for (var index = 0; index < parts.Length; index++)
        {
            if (!NodeName.TryParse(parts[index], out var name))
            {
                reason = $"'{parts[index]}' is not a valid node name";
                return false;
            }

            parsed[index] = name;
        }

        path = new NodePath(parsed);
        reason = string.Empty;
        return true;
    }

    // Written by hand because the generated equality would compare the segment array by reference.
    public bool Equals(NodePath? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public override string ToString() => Value;
}
