using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Tests;

/// <summary>Canned values, so a test only spells out what it is actually about.</summary>
public static class Any
{
    /// <summary>A person.</summary>
    public static readonly ActorId Human = ActorId.Parse("Leon");

    /// <summary>A client application registering its keys.</summary>
    public static readonly ActorId Client = ActorId.Parse("billing-api@v1");

    public static readonly DateTimeOffset At = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset Later(int minutes) => At.AddMinutes(minutes);

    public static NodeName Name(string value) => NodeName.Parse(value);

    public static ConfigKeyName Key(string value) => ConfigKeyName.Parse(value);

    public static KeyDeclaration Declare(string name, ConfigValueKind kind) => new(Key(name), kind);

    public static GroupingNode Root() => GroupingNode.CreateRoot(Human, At);
}
