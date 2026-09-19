namespace org.g14.Configurino.Domain.ConfigTree.Config;

/// <summary>One key being given a value as part of a change set.</summary>
public sealed record KeyAssignment
{
    public KeyAssignment(EntryKey name, EntryValue value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        Name = name;
        Value = value;
    }

    public EntryKey Name { get; }

    public EntryValue Value { get; }

    public override string ToString() => $"{Name} = {Value}";
}
