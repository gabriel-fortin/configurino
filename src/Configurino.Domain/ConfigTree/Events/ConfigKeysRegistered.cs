using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.ConfigTree.Events;

/// <summary>
/// A client application registered its keys and the schema actually moved. A registration that changed
/// nothing — the ordinary case when a service redeploys unchanged — raises nothing at all.
/// </summary>
public sealed record ConfigKeysRegistered : IDomainEvent
{
    private readonly KeyChange[] schemaChanges;
    private readonly ValueChange[] clearedValues;

    public ConfigKeysRegistered(
        NodeId nodeId,
        ChangeSetId changeSetId,
        ActorId registrant,
        DateTimeOffset occurredAt,
        int version,
        IReadOnlyCollection<KeyChange> schemaChanges,
        IReadOnlyCollection<ValueChange> clearedValues)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        ArgumentNullException.ThrowIfNull(changeSetId);
        ArgumentNullException.ThrowIfNull(registrant);
        ArgumentNullException.ThrowIfNull(schemaChanges);
        ArgumentNullException.ThrowIfNull(clearedValues);

        NodeId = nodeId;
        ChangeSetId = changeSetId;
        Registrant = registrant;
        OccurredAt = occurredAt;
        Version = version;
        this.schemaChanges = [.. schemaChanges];
        this.clearedValues = [.. clearedValues];
    }

    public NodeId NodeId { get; }

    public ChangeSetId ChangeSetId { get; }

    /// <summary>The client application that registered.</summary>
    public ActorId Registrant { get; }

    public DateTimeOffset OccurredAt { get; }

    public int Version { get; }

    public IReadOnlyList<KeyChange> SchemaChanges => schemaChanges;

    /// <summary>
    /// Values dropped because their key was declared with a different type. This is the one way a
    /// redeploy can blank a value somebody set, which is why it travels in the event rather than only
    /// in the log.
    /// </summary>
    public IReadOnlyList<ValueChange> ClearedValues => clearedValues;

    public IReadOnlyList<EntryKey> Added => KeysWhere(KeyChangeKind.Added);

    public IReadOnlyList<EntryKey> Reactivated => KeysWhere(KeyChangeKind.Reactivated);

    public IReadOnlyList<EntryKey> KindChanged => KeysWhere(KeyChangeKind.KindChanged);

    public IReadOnlyList<EntryKey> MarkedObsolete => KeysWhere(KeyChangeKind.MarkedObsolete);

    private EntryKey[] KeysWhere(KeyChangeKind change) =>
        [.. schemaChanges.Where(schemaChange => schemaChange.Change == change).Select(schemaChange => schemaChange.Key)];
}
