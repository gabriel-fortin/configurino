using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Repositories;

/// <summary>
/// Turns paths into nodes and lists what a node contains — the read side of browsing the tree.
/// </summary>
/// <remarks>
/// <para>
/// <b>Uniqueness contract.</b> Two nodes under the same parent may never share a name, whatever kind
/// they are: <c>/prod/billing</c> cannot be a grouping node and a config node at the same time. The
/// domain cannot enforce this, because a grouping node does not hold its children, so an implementation
/// must — a unique constraint over (parent id, name) — and must surface a violation as
/// <see cref="Exceptions.NodeNameAlreadyUsedException"/> rather than letting a storage error escape.
/// Two people creating the same node at the same moment is exactly the case this catches.
/// </para>
/// <para>
/// Nodes are never deleted, so an existing node always has an unbroken line of parents up to the root.
/// Anything added later that deletes nodes has to keep that true.
/// </para>
/// </remarks>
public interface INodeDirectory
{
    /// <summary>Finds the node a path addresses, or <c>null</c> when nothing is there.</summary>
    Task<NodeId?> Resolve(NodePath path, CancellationToken cancellationToken = default);

    /// <summary>Rebuilds the path that addresses a node, for showing it to a person.</summary>
    Task<NodePath?> GetPath(NodeId id, CancellationToken cancellationToken = default);

    /// <summary>Tells which kind a node is without loading it.</summary>
    Task<NodeKind?> GetKind(NodeId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists what sits directly under a node, ordered by name so paging through a large group is stable.
    /// A config node has no children and always answers with nothing.
    /// </summary>
    Task<IReadOnlyList<NodeSummary>> GetChildren(NodeId id, CancellationToken cancellationToken = default);
}
