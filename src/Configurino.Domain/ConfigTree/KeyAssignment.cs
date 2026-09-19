namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>One key being given a value as part of a change set.</summary>
public sealed record KeyAssignment
{
    public KeyAssignment(ConfigKeyName name, ConfigValue value)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(value);

        Name = name;
        Value = value;
    }

    public ConfigKeyName Name { get; }

    public ConfigValue Value { get; }

    public override string ToString() => $"{Name} = {Value}";
}
