using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Access.Events;

/// <summary>
/// An actor's rights on a node were set to those of a role — given, changed or, with
/// <see cref="Role.NoAccess"/>, taken away.
/// </summary>
public sealed record RoleSet : IDomainEvent
{
    public RoleSet(
        NodeId nodeId,
        ActorId actor,
        NodeRight previous,
        NodeRight current,
        ActorId by,
        DateTimeOffset occurredAt,
        int version)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(by);

        NodeId = nodeId;
        Actor = actor;
        Previous = previous;
        Current = current;
        By = by;
        OccurredAt = occurredAt;
        Version = version;
    }

    public NodeId NodeId { get; }

    /// <summary>Whose rights were set.</summary>
    public ActorId Actor { get; }

    /// <summary>What the actor held on the node before; <see cref="NodeRight.None"/> if nothing.</summary>
    public NodeRight Previous { get; }

    /// <summary>What the actor holds on the node now; <see cref="NodeRight.None"/> if nothing.</summary>
    public NodeRight Current { get; }

    /// <summary>Who set them.</summary>
    public ActorId By { get; }

    public DateTimeOffset OccurredAt { get; }

    public int Version { get; }
}
