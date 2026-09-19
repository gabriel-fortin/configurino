using System.Diagnostics.CodeAnalysis;

namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// Everything a client application needs from a config node in one read: the whole key set and the
/// version it was read at.
/// </summary>
/// <remarks>
/// <see cref="Version"/> is what a client sends back to ask "has anything changed since?", so polling
/// costs almost nothing when the answer is no.
/// </remarks>
public sealed class ConfigSnapshot
{
    private readonly ConfigKeyView[] _keys;

    public ConfigSnapshot(NodeId nodeId, int version, IReadOnlyCollection<ConfigKeyView> keys)
    {
        ArgumentNullException.ThrowIfNull(nodeId);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentOutOfRangeException.ThrowIfNegative(version);

        NodeId = nodeId;
        Version = version;
        _keys = [.. keys];
    }

    public NodeId NodeId { get; }

    public int Version { get; }

    /// <summary>
    /// Every key, obsolete ones included. They are kept deliberately: an older deployment still running
    /// may be reading a key the newest build stopped declaring, and dropping it here would break exactly
    /// the compatibility that keeping obsolete keys exists to provide.
    /// </summary>
    public IReadOnlyList<ConfigKeyView> Keys => _keys;

    public IEnumerable<ConfigKeyView> ActiveKeys => _keys.Where(key => key.Status == KeyStatus.Active);
}

/// <summary>One key as it appears in a snapshot.</summary>
public sealed class ConfigKeyView
{
    public ConfigKeyView(
        ConfigKeyName name,
        ConfigValueKind kind,
        KeyStatus status,
        ConfigValue? value,
        ChangeStamp lastChange)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(lastChange);

        Name = name;
        Kind = kind;
        Status = status;
        Value = value;
        LastChange = lastChange;
    }

    public ConfigKeyName Name { get; }

    public ConfigValueKind Kind { get; }

    public KeyStatus Status { get; }

    /// <summary>The value, or <c>null</c> when it has never been set. Guard reads with <see cref="IsSet"/>.</summary>
    public ConfigValue? Value { get; }

    public ChangeStamp LastChange { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSet => Value is not null;

    public override string ToString() => $"{Name} : {Kind} = {Value?.ToString() ?? "not set"} ({Status})";
}
