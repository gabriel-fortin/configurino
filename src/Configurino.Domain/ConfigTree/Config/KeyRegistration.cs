namespace org.g14.Configurino.Domain.ConfigTree.Config;

/// <summary>One key in a client application's registration: what it is called and what type it holds.</summary>
public sealed record KeyRegistration
{
    public KeyRegistration(EntryKey name, ValueKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Kind = kind;
    }

    public EntryKey Name { get; }

    public ValueKind Kind { get; }

    public override string ToString() => $"{Name} : {Kind}";
}
