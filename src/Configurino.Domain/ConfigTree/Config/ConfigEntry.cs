using org.g14.Configurino.Domain.ConfigTree.Changes;

namespace org.g14.Configurino.Domain.ConfigTree.Config;

/// <summary>
/// One key inside a <see cref="ConfigNode"/>.
/// The result of what a client application declared, and what value a person optionally has put in it.
/// </summary>
/// <remarks>
/// An entity of the Config Node aggregate. Instances are reached only through
/// <see cref="ConfigNode"/>, which is the single thing allowed to move them between states — every
/// change there goes through one code path, so a change can never be applied without being recorded.
/// </remarks>
public sealed class ConfigEntry
{
    public ConfigEntry(EntryKey name, ValueKind kind, ChangeStamp declaredAt)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(declaredAt);

        Name = name;
        Kind = kind;
        Status = KeyStatus.Active;
        LastChange = declaredAt;
    }

    public EntryKey Name { get; }

    /// <summary>The type declared for this key. Changes only through a registration, never through an edit.</summary>
    public ValueKind Kind { get; private set; }

    public KeyStatus Status { get; private set; }

    /// <summary>The current value, or <c>null</c> when nobody has set one.</summary>
    public EntryValue? Value { get; private set; }

    /// <summary>Who last changed this key and when.</summary>
    public ChangeStamp LastChange { get; private set; }

    public void Reactivate(ChangeStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(stamp);

        Status = KeyStatus.Active;
        LastChange = stamp;
    }

    public void MarkObsolete(ChangeStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(stamp);

        Status = KeyStatus.Obsolete;
        LastChange = stamp;
    }

    /// <summary>
    /// Takes the newly declared type and drops whatever value was there, so a value of the old type can
    /// never be served against the new declaration. The dropped value is recorded by the caller.
    /// </summary>
    public void ChangeKind(ValueKind kind, ChangeStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(stamp);

        Kind = kind;
        Value = null;
        Status = KeyStatus.Active;
        LastChange = stamp;
    }

    /// <summary>Sets the value, or clears it when given <c>null</c>.</summary>
    public void Assign(EntryValue? value, ChangeStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(stamp);

        if (value is not null && value.Kind != Kind)
        {
            throw new ArgumentException($"Key '{Name}' holds {Kind}, not {value.Kind}.", nameof(value));
        }

        Value = value;
        LastChange = stamp;
    }

    public override string ToString() => $"{Name} : {Kind} = {Value?.ToString() ?? "not set"} ({Status})";
}
