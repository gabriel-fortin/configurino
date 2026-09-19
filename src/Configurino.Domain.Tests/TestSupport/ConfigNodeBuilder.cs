using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Tests;

/// <summary>
/// Builds a config node that is already set up, and throws away the events that setting it up raised, so
/// a test can assert on the events of the one thing it is exercising.
/// </summary>
public sealed class ConfigNodeBuilder
{
    private readonly List<KeyDeclaration> declarations = [];
    private readonly List<KeyAssignment> assignments = [];

    public static ConfigNodeBuilder AConfigNode() => new();

    public ConfigNodeBuilder WithKey(string name, ConfigValueKind kind)
    {
        declarations.Add(Any.Declare(name, kind));
        return this;
    }

    public ConfigNodeBuilder WithValue(string name, ConfigValue value)
    {
        assignments.Add(new KeyAssignment(Any.Key(name), value));
        return this;
    }

    public ConfigNode Build()
    {
        var node = Any.Root().CreateChildConfig(Any.Name("billing-api"), Any.Human, Any.At);

        if (declarations.Count > 0)
        {
            node.RegisterKeys(declarations, Any.Client, Any.At);
        }

        if (assignments.Count > 0)
        {
            node.SetValues(assignments, Any.Human, Any.At, ChangeReason.Manual());
        }

        node.DequeueDomainEvents();

        return node;
    }
}
