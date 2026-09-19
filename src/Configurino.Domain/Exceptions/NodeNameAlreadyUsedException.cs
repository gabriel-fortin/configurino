using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// A sibling of the given name already exists under the parent. Raised by the persistence layer when
/// it enforces the uniqueness contract documented on <c>INodeDirectory</c>, which the domain cannot
/// check on its own because a grouping node does not hold its children.
/// </summary>
public sealed class NodeNameAlreadyUsedException : DomainException
{
    public NodeNameAlreadyUsedException(NodeId parentId, NodeName name)
        : base($"A node named '{name}' already exists under parent {parentId}.")
    {
        ParentId = parentId;
        Name = name;
    }

    public NodeId ParentId { get; }

    public NodeName Name { get; }
}
