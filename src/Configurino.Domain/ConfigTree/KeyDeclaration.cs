namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>One key in a client application's registration: what it is called and what type it holds.</summary>
public sealed record KeyDeclaration
{
    public KeyDeclaration(ConfigKeyName name, ConfigValueKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Kind = kind;
    }

    public ConfigKeyName Name { get; }

    public ConfigValueKind Kind { get; }

    public override string ToString() => $"{Name} : {Kind}";
}
