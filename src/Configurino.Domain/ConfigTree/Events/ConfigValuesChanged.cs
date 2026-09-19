using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Domain.ConfigTree.Events;

/// <summary>
/// A person changed one or more values. Everything changed together travels in one event under one
/// change set id, so it can be shown — and undone — as the single edit it was.
/// </summary>
public sealed record ConfigValuesChanged : IDomainEvent
{
    private readonly ValueChange[] changes;

    public ConfigValuesChanged(
        NodeId nodeId,
        ChangeSetId changeSetId,
        ActorId by,
        DateTimeOffset occurredAt,
        int version,
        ChangeReason reason,
        IReadOnlyCollection<ValueChange> changes)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        ArgumentNullException.ThrowIfNull(changeSetId);
        ArgumentNullException.ThrowIfNull(by);
        ArgumentNullException.ThrowIfNull(reason);
        ArgumentNullException.ThrowIfNull(changes);

        NodeId = nodeId;
        ChangeSetId = changeSetId;
        By = by;
        OccurredAt = occurredAt;
        Version = version;
        Reason = reason;
        this.changes = [.. changes];
    }

    public NodeId NodeId { get; }

    public ChangeSetId ChangeSetId { get; }

    public ActorId By { get; }

    public DateTimeOffset OccurredAt { get; }

    public int Version { get; }

    public ChangeReason Reason { get; }

    public IReadOnlyList<ValueChange> Changes => changes;
}
