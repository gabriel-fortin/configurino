using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Domain.ConfigTree.Events;

/// <summary>
/// A client application registered its keys and the schema actually moved. A registration that changed
/// nothing — the ordinary case when a service redeploys unchanged — raises nothing at all.
/// </summary>
public sealed record ConfigKeysRegistered : IDomainEvent
{
    private readonly SchemaChange[] schemaChanges;
    private readonly ValueChange[] clearedValues;

    public ConfigKeysRegistered(
        NodeId nodeId,
        ChangeSetId changeSetId,
        ActorId registrant,
        DateTimeOffset occurredAt,
        int version,
        IReadOnlyCollection<SchemaChange> schemaChanges,
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

    public IReadOnlyList<SchemaChange> SchemaChanges => schemaChanges;

    /// <summary>
    /// Values dropped because their key was declared with a different type. This is the one way a
    /// redeploy can blank a value somebody set, which is why it travels in the event rather than only
    /// in the log.
    /// </summary>
    public IReadOnlyList<ValueChange> ClearedValues => clearedValues;

    public IReadOnlyList<ConfigKeyName> Added => KeysWhere(SchemaChangeKind.Added);

    public IReadOnlyList<ConfigKeyName> Reactivated => KeysWhere(SchemaChangeKind.Reactivated);

    public IReadOnlyList<ConfigKeyName> KindChanged => KeysWhere(SchemaChangeKind.KindChanged);

    public IReadOnlyList<ConfigKeyName> MarkedObsolete => KeysWhere(SchemaChangeKind.MarkedObsolete);

    private ConfigKeyName[] KeysWhere(SchemaChangeKind change) =>
        [.. schemaChanges.Where(schemaChange => schemaChange.Change == change).Select(schemaChange => schemaChange.Key)];
}
