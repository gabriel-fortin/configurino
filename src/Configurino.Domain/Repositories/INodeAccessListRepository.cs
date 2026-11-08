using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Repositories;

/// <summary>Stores and retrieves the access lists of nodes.</summary>
public interface INodeAccessListRepository
{
    /// <summary>Loads the access list of a node, or <c>null</c> when nothing was ever granted on it.</summary>
    Task<NodeAccessList?> Find(NodeId nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the access lists of several nodes at once — typically a node and everything above it, which
    /// is what every access check needs. Nodes without a list are simply left out.
    /// </summary>
    Task<IReadOnlyList<NodeAccessList>> FindMany(
        IReadOnlyCollection<NodeId> nodeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the first list of a node, and in the same transaction records the grants its events describe,
    /// so the audit trail cannot miss one.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="Exceptions.ConcurrencyConflictException"/> when the node already has a list:
    /// two first grants at the same moment is exactly what this catches.
    /// </remarks>
    Task Add(NodeAccessList list, CancellationToken cancellationToken = default);

    /// <summary>Saves a changed list, and in the same transaction records the changes its events describe.</summary>
    /// <remarks>
    /// Throws <see cref="Exceptions.ConcurrencyConflictException"/> when the stored version has moved on.
    /// </remarks>
    Task Save(NodeAccessList list, CancellationToken cancellationToken = default);
}
