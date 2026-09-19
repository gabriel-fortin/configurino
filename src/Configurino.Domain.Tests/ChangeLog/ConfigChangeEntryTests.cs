using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Events;

namespace org.g14.Configurino.Domain.Tests;

public sealed class ConfigChangeEntryTests
{
    [Fact]
    public void Records_what_a_person_changed()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), ConfigValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual("go live"));

        var entry = ConfigChangeEntry.From(EventFrom<ConfigValuesChanged>(node));

        Assert.Equal(node.Id, entry.NodeId);
        Assert.Equal(Any.Human, entry.By);
        Assert.Equal(Any.At, entry.At);
        Assert.Equal(node.Version, entry.NodeVersion);
        Assert.Equal(ChangeReason.Manual("go live"), entry.Reason);
        Assert.Empty(entry.SchemaChanges);

        var change = Assert.Single(entry.ValueChanges);
        Assert.Null(change.Previous);
        Assert.Equal(ConfigValue.Of(30L), change.Current);
    }

    [Fact]
    public void Records_what_a_registration_changed()
    {
        var node = Node();
        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.String)], Any.Client, Any.Later(60));

        var entry = ConfigChangeEntry.From(EventFrom<ConfigKeysRegistered>(node));

        Assert.Equal(Any.Client, entry.By);
        Assert.Equal(ChangeReason.Registration, entry.Reason);

        var schemaChange = Assert.Single(entry.SchemaChanges);
        Assert.Equal(SchemaChangeKind.KindChanged, schemaChange.Change);
    }

    [Fact]
    public void Points_at_the_node_by_identity_rather_than_by_where_it_sits()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), ConfigValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());

        var entry = ConfigChangeEntry.From(EventFrom<ConfigValuesChanged>(node));

        // History stays attached to the node itself, not to a path that could stop being true.
        Assert.Equal(node.Id, entry.NodeId);
    }

    [Fact]
    public void Undoing_a_change_is_recorded_as_a_new_change_that_says_so()
    {
        var node = Node();

        node.SetValue(Any.Key("timeout"), ConfigValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());
        var first = ConfigChangeEntry.From(EventFrom<ConfigValuesChanged>(node));

        node.SetValue(Any.Key("timeout"), ConfigValue.Of(60L), Any.Human, Any.Later(10), ChangeReason.Manual());
        var second = ConfigChangeEntry.From(EventFrom<ConfigValuesChanged>(node));

        // Rolling back is a forward operation: put the old value back and say why. History is never rewritten.
        var restored = second.ValueChanges.Single().Previous;
        Assert.NotNull(restored);
        node.SetValue(Any.Key("timeout"), restored, Any.Human, Any.Later(20), ChangeReason.Rollback(second.Id));
        var third = ConfigChangeEntry.From(EventFrom<ConfigValuesChanged>(node));

        Assert.Equal(KeyReadResult.Set(ConfigValue.Of(30L)), node.TryGetValue(Any.Key("timeout")));

        var rollback = Assert.IsType<RollbackChange>(third.Reason);
        Assert.Equal(second.Id, rollback.Target);

        var versions = new[] { first.NodeVersion, second.NodeVersion, third.NodeVersion };
        Assert.Equal(versions.OrderBy(version => version), versions);
    }

    [Fact]
    public void Cannot_roll_back_a_key_that_has_since_gone_obsolete()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), ConfigValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());
        node.RegisterKeys([Any.Declare("other", ConfigValueKind.String)], Any.Client, Any.Later(10));

        // Bringing the key back is a registration, which is the client application's business, not a
        // human's — so the rollback fails rather than reviving a key nothing declares.
        Assert.Throws<Exceptions.ObsoleteKeyException>(() => node.SetValue(
            Any.Key("timeout"),
            ConfigValue.Of(15L),
            Any.Human,
            Any.Later(20),
            ChangeReason.Rollback(ChangeSetId.New())));
    }

    private static ConfigNode Node() => ConfigNodeBuilder.AConfigNode()
        .WithKey("timeout", ConfigValueKind.Integer)
        .Build();

    private static TEvent EventFrom<TEvent>(ConfigNode node)
        where TEvent : class
    {
        var raised = node.DequeueDomainEvents();
        return Assert.IsType<TEvent>(Assert.Single(raised));
    }
}
