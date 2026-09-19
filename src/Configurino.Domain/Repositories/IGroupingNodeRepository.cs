using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Repositories;

/// <summary>Stores and retrieves grouping nodes.</summary>
/// <remarks>
/// There is no update: a grouping node never changes after it is created. Loading one is how a caller
/// earns the right to create something under it.
/// </remarks>
public interface IGroupingNodeRepository
{
    /// <summary>Loads a grouping node, or <c>null</c> when there is none with that id.</summary>
    Task<GroupingNode?> Find(NodeId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a newly created grouping node. Throws
    /// <see cref="Exceptions.NodeNameAlreadyUsedException"/> if a sibling already has that name.
    /// </summary>
    Task Add(GroupingNode node, CancellationToken cancellationToken = default);
}
