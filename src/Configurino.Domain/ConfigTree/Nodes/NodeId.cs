namespace org.g14.Configurino.Domain.ConfigTree.Nodes;

/// <summary>
/// The identity of a node. Deliberately independent of where the node sits in the tree: a path is how
/// callers address a node, but this is what the change log and every other reference points at, so a
/// node keeps its history even if its name or place in the tree ever changes.
/// </summary>
public sealed record NodeId
{
    /// <summary>
    /// The one grouping node that exists by default. Fixed rather than generated so that creating it is
    /// idempotent and "there is exactly one root" needs no extra rule.
    /// </summary>
    public static readonly NodeId Root = new(new Guid("00000000-0000-7000-8000-000000000014"));

    private NodeId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsRoot => Value == Root.Value;

    /// <summary>Mints an identity for a node being created. Version 7 so ids sort by creation time.</summary>
    public static NodeId New() => new(Guid.CreateVersion7());

    /// <summary>Rebuilds an identity read back from storage.</summary>
    public static NodeId From(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("A node id cannot be empty.", nameof(value))
            : new NodeId(value);

    public override string ToString() => Value.ToString();
}
