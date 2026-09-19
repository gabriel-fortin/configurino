namespace org.g14.Configurino.Domain.ConfigTree.Nodes;

/// <summary>
/// What browsing a grouping node returns for each child: enough to list and navigate the tree without
/// loading any child aggregate.
/// </summary>
public sealed record NodeSummary
{
    public NodeSummary(NodeId id, NodeName name, NodeKind kind)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);

        Id = id;
        Name = name;
        Kind = kind;
    }

    public NodeId Id { get; }

    public NodeName Name { get; }

    public NodeKind Kind { get; }
}
