using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Events;

namespace org.g14.Configurino.Domain.Tests;

/// <summary>
/// A client application may change the type of a key it already declared. The new type wins and the
/// value goes, so a value of the old type can never be served against the new declaration — and the same
/// rule applies whether the key was still in use or had gone obsolete.
/// </summary>
public sealed class ConfigNodeKindChangeTests
{
    [Fact]
    public void Changing_the_type_of_a_live_key_takes_the_new_type_and_drops_the_value()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithValue("timeout", ConfigValue.Of(30L))
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.String)], Any.Client, Any.Later(60));

        var timeout = KeyIn(node, "timeout");
        Assert.Equal(ConfigValueKind.String, timeout.Kind);
        Assert.Equal(KeyStatus.Active, timeout.Status);
        Assert.False(timeout.IsSet);
        Assert.Equal(KeyReadResult.NotSet, node.TryGetValue(Any.Key("timeout")));
    }

    [Fact]
    public void Changing_the_type_of_an_obsolete_key_does_exactly_the_same()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithValue("timeout", ConfigValue.Of(30L))
            .Build();

        node.RegisterKeys([Any.Declare("other", ConfigValueKind.String)], Any.Client, Any.Later(60));
        Assert.Equal(KeyStatus.Obsolete, KeyIn(node, "timeout").Status);

        node.RegisterKeys(
            [Any.Declare("timeout", ConfigValueKind.Boolean), Any.Declare("other", ConfigValueKind.String)],
            Any.Client,
            Any.Later(120));

        var timeout = KeyIn(node, "timeout");
        Assert.Equal(ConfigValueKind.Boolean, timeout.Kind);
        Assert.Equal(KeyStatus.Active, timeout.Status);
        Assert.False(timeout.IsSet);
    }

    [Fact]
    public void Says_out_loud_which_values_it_dropped()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithValue("timeout", ConfigValue.Of(30L))
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.String)], Any.Client, Any.Later(60));

        var registered = Assert.IsType<ConfigKeysRegistered>(Assert.Single(node.DequeueDomainEvents()));

        Assert.Equal([Any.Key("timeout")], registered.KindChanged);

        // This is the one way a redeploy can blank something a person set, so it travels in the event
        // rather than only being discoverable by diffing the log.
        var cleared = Assert.Single(registered.ClearedValues);
        Assert.Equal(Any.Key("timeout"), cleared.Key);
        Assert.Equal(ConfigValue.Of(30L), cleared.Previous);
        Assert.Null(cleared.Current);

        var change = Assert.Single(registered.SchemaChanges);
        Assert.Equal(ConfigValueKind.Integer, change.PreviousKind);
        Assert.Equal(ConfigValueKind.String, change.Kind);
    }

    [Fact]
    public void Reports_no_dropped_value_when_there_was_none_to_drop()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.String)], Any.Client, Any.Later(60));

        var registered = Assert.IsType<ConfigKeysRegistered>(Assert.Single(node.DequeueDomainEvents()));
        Assert.Equal([Any.Key("timeout")], registered.KindChanged);
        Assert.Empty(registered.ClearedValues);
    }

    [Fact]
    public void Lets_a_value_of_the_new_type_be_set_straight_away()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithValue("timeout", ConfigValue.Of(30L))
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.String)], Any.Client, Any.Later(60));
        node.SetValue(Any.Key("timeout"), ConfigValue.Of("30s"), Any.Human, Any.Later(90), ChangeReason.Manual());

        Assert.Equal(KeyReadResult.Set(ConfigValue.Of("30s")), node.TryGetValue(Any.Key("timeout")));
    }

    private static ConfigKeyView KeyIn(ConfigNode node, string name) =>
        node.Snapshot().Keys.Single(key => key.Name == Any.Key(name));
}
