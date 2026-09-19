using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>One key's schema moving, as recorded in the change log.</summary>
public sealed record SchemaChange
{
    private SchemaChange(EntryKey key, SchemaChangeKind change, ValueKind? previousKind, ValueKind newKind)
    {
        ArgumentNullException.ThrowIfNull(key);

        Key = key;
        Change = change;
        PreviousKind = previousKind;
        NewKind = newKind;
    }

    public EntryKey Key { get; }

    public SchemaChangeKind Change { get; }

    /// <summary>The type the key had before, or <c>null</c> when this registration introduced it.</summary>
    public ValueKind? PreviousKind { get; }

    /// <summary>The type the key has now.</summary>
    public ValueKind NewKind { get; }

    public override string ToString() => $"{Key}: {Change} ({NewKind})";

    public static SchemaChange AddEntry(EntryKey key, ValueKind kind) =>
        new(key, SchemaChangeKind.Added, null, kind);
    
    public static SchemaChange Reactivate(EntryKey key, ValueKind previousKind, ValueKind newKind) =>
        new(key, SchemaChangeKind.Reactivated, previousKind, newKind);
    
    public static SchemaChange ChangeKind(EntryKey key, ValueKind previousKind, ValueKind newKind) =>
        new(key, SchemaChangeKind.KindChanged, previousKind, newKind);
    
    public static SchemaChange MarkObsolete(EntryKey key, ValueKind kind) =>
        new(key, SchemaChangeKind.MarkedObsolete, kind, kind);
    
    
}
