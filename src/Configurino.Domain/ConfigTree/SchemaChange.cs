namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>What a registration did to one key.</summary>
public enum SchemaChangeKind
{
    /// <summary>The key had never been registered here before.</summary>
    Added = 1,

    /// <summary>A key that had gone obsolete is declared again, keeping the value it had.</summary>
    Reactivated = 2,

    /// <summary>
    /// The key is declared with a different type than before. The new type wins and the value is
    /// cleared, so a value of the wrong type can never be served.
    /// </summary>
    KindChanged = 3,

    /// <summary>The key was not declared any more, so it is kept but marked obsolete.</summary>
    MarkedObsolete = 4,
}

/// <summary>One key's schema moving, as recorded in the change log.</summary>
public sealed record SchemaChange
{
    public SchemaChange(ConfigKeyName key, SchemaChangeKind change, ConfigValueKind? previousKind, ConfigValueKind kind)
    {
        ArgumentNullException.ThrowIfNull(key);

        Key = key;
        Change = change;
        PreviousKind = previousKind;
        Kind = kind;
    }

    public ConfigKeyName Key { get; }

    public SchemaChangeKind Change { get; }

    /// <summary>The type the key had before, or <c>null</c> when this registration introduced it.</summary>
    public ConfigValueKind? PreviousKind { get; }

    /// <summary>The type the key has now.</summary>
    public ConfigValueKind Kind { get; }

    public override string ToString() => $"{Key}: {Change} ({Kind})";
}
