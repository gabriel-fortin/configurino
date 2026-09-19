using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Domain.ConfigTree.Events;

/// <summary>A config node was created, empty, waiting for a client application to register its keys.</summary>
public sealed record ConfigNodeCreated : IDomainEvent
{
    public ConfigNodeCreated(NodeId id, NodeId parentId, NodeName name, ActorId by, DateTimeOffset occurredAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(parentId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(by);

        Id = id;
        ParentId = parentId;
        Name = name;
        By = by;
        OccurredAt = occurredAt;
    }

    public NodeId Id { get; }

    public NodeId ParentId { get; }

    public NodeName Name { get; }

    public ActorId By { get; }

    public DateTimeOffset OccurredAt { get; }
}
