using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>
/// One key moving from one value to another, as recorded in the change log.
/// </summary>
/// <remarks>
/// <c>null</c> means "no value" on either side, which is unambiguous here in a way it would not be on a
/// read: within a change the key certainly exists, so the only question is whether it held a value.
/// </remarks>
public sealed record ValueChange
{
    public ValueChange(EntryKey key, EntryValue? previous, EntryValue? current)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (previous == current)
        {
            throw new ArgumentException($"Key '{key}' did not actually change.", nameof(current));
        }

        Key = key;
        Previous = previous;
        Current = current;
    }

    public EntryKey Key { get; }

    /// <summary>What the key held before, or <c>null</c> if it held nothing.</summary>
    public EntryValue? Previous { get; }

    /// <summary>What the key holds now, or <c>null</c> if it was cleared.</summary>
    public EntryValue? Current { get; }

    public bool HadValue => Previous is not null;

    public bool HasValue => Current is not null;

    public override string ToString() =>
        $"{Key}: {Previous?.ToString() ?? "not set"} -> {Current?.ToString() ?? "not set"}";
}
