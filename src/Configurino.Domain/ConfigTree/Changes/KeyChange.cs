using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>One key's schema moving, as recorded in the change log.</summary>
public sealed record KeyChange
{
    private KeyChange(EntryKey key, KeyChangeKind change, ValueKind? previousKind, ValueKind newKind)
    {
        ArgumentNullException.ThrowIfNull(key);

        Key = key;
        Change = change;
        PreviousKind = previousKind;
        NewKind = newKind;
    }

    public EntryKey Key { get; }

    public KeyChangeKind Change { get; }

    /// <summary>The type the key had before, or <c>null</c> when this registration introduced it.</summary>
    public ValueKind? PreviousKind { get; }

    /// <summary>The type the key has now.</summary>
    public ValueKind NewKind { get; }

    public override string ToString() => $"{Key}: {Change} ({NewKind})";

    public static KeyChange AddEntry(EntryKey key, ValueKind kind) =>
        new(key, KeyChangeKind.Added, null, kind);
    
    public static KeyChange Reactivate(EntryKey key, ValueKind previousKind, ValueKind newKind) =>
        new(key, KeyChangeKind.Reactivated, previousKind, newKind);
    
    public static KeyChange ChangeKind(EntryKey key, ValueKind previousKind, ValueKind newKind) =>
        new(key, KeyChangeKind.KindChanged, previousKind, newKind);
    
    public static KeyChange MarkObsolete(EntryKey key, ValueKind kind) =>
        new(key, KeyChangeKind.MarkedObsolete, kind, kind);
    
    
}
