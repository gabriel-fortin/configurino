using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.ChangeLog;

/// <summary>
/// One entry in a config node's history: everything a single operation changed, and why.
/// </summary>
/// <remarks>
/// <para>
/// An aggregate in its own right, and a deliberately tiny one — one entry per change set, never a
/// growing collection. Appending to the history is creating a new entry, so recording a change costs the
/// same on a node changed twice as on one changed ten thousand times.
/// </para>
/// <para>
/// It is built from the events a <see cref="ConfigNode"/> raises, but it must be written in the same
/// transaction as the change it describes. Building it from events dispatched after the commit would
/// lose audit records whenever a process died in between, and an audit trail with holes in it answers
/// nothing.
/// </para>
/// <para>
/// Entries are never changed or removed. Undoing something is a new change that puts the old value back,
/// recorded with <see cref="ChangeReason.Rollback"/> so the history shows what actually happened rather
/// than pretending it never did.
/// </para>
/// </remarks>
public sealed class ConfigChangeEntry
{
    private readonly ValueChange[] valueChanges;
    private readonly KeyChange[] schemaChanges;

    private ConfigChangeEntry(
        ChangeSetId id,
        NodeId nodeId,
        ActorId by,
        DateTimeOffset at,
        int nodeVersion,
        ChangeReason reason,
        ValueChange[] valueChanges,
        KeyChange[] schemaChanges)
    {
        Id = id;
        NodeId = nodeId;
        By = by;
        At = at;
        NodeVersion = nodeVersion;
        Reason = reason;
        this.valueChanges = valueChanges;
        this.schemaChanges = schemaChanges;
    }

    public ChangeSetId Id { get; }

    /// <summary>
    /// The node this happened to, by identity rather than by path, so the history stays attached to the
    /// node whatever it ends up being called.
    /// </summary>
    public NodeId NodeId { get; }

    public ActorId By { get; }

    public DateTimeOffset At { get; }

    /// <summary>The version the node reached with this change, which orders history without trusting clocks.</summary>
    public int NodeVersion { get; }

    public ChangeReason Reason { get; }

    /// <summary>Values that moved. For a registration these are the ones a type change dropped.</summary>
    public IReadOnlyList<ValueChange> ValueChanges => valueChanges;

    /// <summary>Keys whose declaration moved. Empty for an ordinary edit.</summary>
    public IReadOnlyList<KeyChange> SchemaChanges => schemaChanges;

    /// <summary>Records what a person changed.</summary>
    public static ConfigChangeEntry From(ConfigValuesChanged change)
    {
        ArgumentNullException.ThrowIfNull(change);

        return new ConfigChangeEntry(
            change.ChangeSetId,
            change.NodeId,
            change.By,
            change.OccurredAt,
            change.Version,
            change.Reason,
            [.. change.Changes],
            []);
    }

    /// <summary>Records what a client application's registration changed.</summary>
    public static ConfigChangeEntry From(ConfigKeysRegistered registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        return new ConfigChangeEntry(
            registration.ChangeSetId,
            registration.NodeId,
            registration.Registrant,
            registration.OccurredAt,
            registration.Version,
            ChangeReason.Registration,
            [.. registration.ClearedValues],
            [.. registration.SchemaChanges]);
    }

    public override string ToString() =>
        $"{Id} on {NodeId} by {By} at {At:O} (v{NodeVersion}), {Reason}";
}
