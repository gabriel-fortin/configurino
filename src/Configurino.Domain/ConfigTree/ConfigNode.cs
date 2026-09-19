using System.Diagnostics;
using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// The whole configuration of one service. A client application declares which keys exist here; people
/// decide what they hold.
/// </summary>
/// <remarks>
/// <para>
/// A config node is a leaf by construction — it offers no way to create a child, which is how "a config
/// node cannot have children" stays a fact about the types rather than a rule someone has to check.
/// </para>
/// <para>
/// The aggregate holds the current state only. Its full history lives in the change log, assembled from
/// the events raised here, so appending to a two-year audit trail never costs anything at write time.
/// </para>
/// </remarks>
public sealed class ConfigNode : AggregateRoot
{
    private readonly Dictionary<ConfigKeyName, ConfigKey> _keys = [];

    private ConfigNode(NodeId id, NodeId parentId, NodeName name)
        : base(version: 0)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
    }

    public NodeId Id { get; }

    public NodeId ParentId { get; }

    public NodeName Name { get; }

    public int KeyCount => _keys.Count;

    /// <summary>
    /// Creates a config node under a grouping node. Taking the parent itself rather than its id is what
    /// makes it impossible to hang a node under a config node or under nothing at all.
    /// </summary>
    public static ConfigNode CreateUnder(GroupingNode parent, NodeName name, ActorId by, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(by);

        var node = new ConfigNode(NodeId.New(), parent.Id, name);
        node.MarkChanged();
        node.Raise(new ConfigNodeCreated(node.Id, parent.Id, name, by, RequireUtc(at, nameof(at))));

        return node;
    }

    /// <summary>
    /// Declares the keys a client application uses here, merging with what is already registered.
    /// </summary>
    /// <remarks>
    /// Keys are never removed: one that stops being declared is kept and marked obsolete, because an
    /// older deployment still running may be reading it. A key declared with a different type takes the
    /// new type and loses its value — the only way a redeploy can blank something a person set, which is
    /// why it is reported separately in the event it raises.
    /// <para>
    /// Idempotent: registering the same schema twice changes nothing the second time, raises no event and
    /// does not bump the version, so a fleet of pods redeploying does not fill the audit trail with noise.
    /// </para>
    /// </remarks>
    public void RegisterKeys(IReadOnlyCollection<KeyDeclaration> declarations, ActorId registrant, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(declarations);
        ArgumentNullException.ThrowIfNull(registrant);

        var utc = RequireUtc(at, nameof(at));

        if (declarations.Count == 0)
        {
            throw new EmptyRegistrationException();
        }

        var declared = new Dictionary<ConfigKeyName, ConfigValueKind>();

        foreach (var declaration in declarations)
        {
            ArgumentNullException.ThrowIfNull(declaration);

            if (!declared.TryAdd(declaration.Name, declaration.Kind))
            {
                throw new DuplicateKeyDeclarationException(declaration.Name);
            }
        }

        // Work out what would change before changing anything: that way a rejected registration leaves
        // the node untouched, and one that changes nothing leaves no trace at all.
        var schemaChanges = new List<SchemaChange>();
        var clearedValues = new List<ValueChange>();

        foreach (var declaration in declarations)
        {
            if (!_keys.TryGetValue(declaration.Name, out var existing))
            {
                schemaChanges.Add(new SchemaChange(declaration.Name, SchemaChangeKind.Added, null, declaration.Kind));
            }
            else if (existing.Kind != declaration.Kind)
            {
                schemaChanges.Add(
                    new SchemaChange(declaration.Name, SchemaChangeKind.KindChanged, existing.Kind, declaration.Kind));

                // changing the kind clears the value (if present)
                if (existing.Value is not null)
                {
                    clearedValues.Add(new ValueChange(declaration.Name, existing.Value, null));
                }
            }
            else if (existing.Status == KeyStatus.Obsolete)
            {
                schemaChanges.Add(
                    new SchemaChange(declaration.Name, SchemaChangeKind.Reactivated, existing.Kind, declaration.Kind));
            }
        }

        var abandoned = _keys.Values
            .Where(key => key.Status == KeyStatus.Active && !declared.ContainsKey(key.Name))
            .OrderBy(key => key.Name.Value, StringComparer.Ordinal);

        foreach (var key in abandoned)
        {
            schemaChanges.Add(new SchemaChange(key.Name, SchemaChangeKind.MarkedObsolete, key.Kind, key.Kind));
        }

        if (schemaChanges.Count == 0)
        {
            return;
        }

        MarkChanged();

        var stamp = new ChangeStamp(registrant, utc, Version);

        foreach (var change in schemaChanges)
        {
            switch (change.Change)
            {
                case SchemaChangeKind.Added:
                    _keys.Add(change.Key, new ConfigKey(change.Key, change.Kind, stamp));
                    break;

                case SchemaChangeKind.KindChanged:
                    _keys[change.Key].ChangeKind(change.Kind, stamp);
                    break;

                case SchemaChangeKind.Reactivated:
                    _keys[change.Key].Reactivate(stamp);
                    break;

                case SchemaChangeKind.MarkedObsolete:
                    _keys[change.Key].MarkObsolete(stamp);
                    break;

                default:
                    throw new UnreachableException($"Unhandled schema change {change.Change}.");
            }
        }

        Raise(new ConfigKeysRegistered(Id, ChangeSetId.New(), registrant, utc, Version, schemaChanges, clearedValues));
    }

    /// <summary>Gives one key a value.</summary>
    public void SetValue(ConfigKeyName key, ConfigValue value, ActorId by, DateTimeOffset at, ChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        SetValues([new KeyAssignment(key, value)], by, at, reason);
    }

    /// <summary>
    /// Gives several keys their values as one change. Everything moves together under a single change set
    /// id, so what a person did in one edit can be shown — and undone — as one thing.
    /// </summary>
    public void SetValues(
        IReadOnlyCollection<KeyAssignment> assignments,
        ActorId by,
        DateTimeOffset at,
        ChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        Dictionary<ConfigKeyName, ConfigValue?> targets = assignments
            .ToDictionary(x => x.Name, ConfigValue? (x) => x.Value);

        ApplyChangeSet(targets, by, at, reason);
    }

    /// <summary>
    /// Takes the values back off the given keys, so they read as not set again. A mistaken value has to be
    /// undoable, which means "not set" must be somewhere you can get back to and not just where you start.
    /// </summary>
    public void ClearValues(
        IReadOnlyCollection<ConfigKeyName> keysToClear,
        ActorId by,
        DateTimeOffset at,
        ChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(keysToClear);

        Dictionary<ConfigKeyName, ConfigValue?> targets = keysToClear.ToDictionary(x => x, ConfigValue? (_) => null);

        ApplyChangeSet(targets, by, at, reason);
    }

    /// <summary>
    /// Reads one key. The three outcomes are kept apart on purpose: a key nobody registered and a key
    /// nobody has filled in call for very different things from a client application.
    /// </summary>
    public KeyReadResult TryGetValue(ConfigKeyName key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_keys.TryGetValue(key, out var registered))
        {
            return KeyReadResult.Unknown;
        }

        return registered.Value is null ? KeyReadResult.NotSet : KeyReadResult.Set(registered.Value);
    }

    /// <summary>Reads the whole configuration at once, which is how a client application normally reads it.</summary>
    public ConfigSnapshot Snapshot() =>
        new(
            Id,
            Version,
            [
                .. _keys.Values
                    .OrderBy(key => key.Name.Value, StringComparer.Ordinal)
                    .Select(key => new ConfigKeyView(key.Name, key.Kind, key.Status, key.Value, key.LastChange)),
            ]);

    /// <summary>
    /// The one path through which a value ever moves, so nothing can change without being recorded.
    /// </summary>
    private void ApplyChangeSet(
        IReadOnlyDictionary<ConfigKeyName, ConfigValue?> targets,
        ActorId by,
        DateTimeOffset at,
        ChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(by);
        ArgumentNullException.ThrowIfNull(reason);

        DateTimeOffset utc = RequireUtc(at, nameof(at));

        // keys we've seen so far in 'targets'
        var seen = new HashSet<ConfigKeyName>();

        foreach (var (key, value) in targets)
        {
            if (!seen.Add(key))
            {
                throw new DuplicateAssignmentException(key);
            }

            if (!_keys.TryGetValue(key, out ConfigKey? existing))
            {
                throw new UnknownConfigKeyException(key);
            }

            if (existing.Status == KeyStatus.Obsolete)
            {
                throw new ObsoleteKeyException(key);
            }

            if (value is not null && value.Kind != existing.Kind)
            {
                throw new ValueKindMismatchException(key, existing.Kind, value.Kind);
            }
        }

        // Assignments that change nothing are dropped, so pressing save without editing anything does not
        // fill the audit trail with entries saying nothing happened.
        var changes = targets
            .Where(target => _keys[target.Key].Value != target.Value)
            .Select(target => new ValueChange(target.Key, _keys[target.Key].Value, target.Value))
            .ToArray();

        if (changes.Length == 0)
        {
            return;
        }

        MarkChanged();

        var stamp = new ChangeStamp(by, utc, Version);

        foreach (var change in changes)
        {
            _keys[change.Key].Assign(change.Current, stamp);
        }

        Raise(new ConfigValuesChanged(Id, ChangeSetId.New(), by, utc, Version, reason, changes));
    }

    public override string ToString() => $"{Name} ({Id}), {_keys.Count} key(s) at v{Version}";
}
