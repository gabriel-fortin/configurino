using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Domain.ConfigTree.Events;

/// <summary>A grouping node was created.</summary>
public sealed record GroupingNodeCreated : IDomainEvent
{
    public GroupingNodeCreated(NodeId id, NodeId? parentId, NodeName? name, ActorId by, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(by);

        Id = id;
        ParentId = parentId;
        Name = name;
        By = by;
        OccurredAt = occurredAt;
    }

    public NodeId Id { get; }

    /// <summary><c>null</c> for the root, which is the one node that sits under nothing.</summary>
    public NodeId? ParentId { get; }

    /// <summary><c>null</c> for the root, which has no name of its own.</summary>
    public NodeName? Name { get; }

    public ActorId By { get; }

    public DateTimeOffset OccurredAt { get; }
}
