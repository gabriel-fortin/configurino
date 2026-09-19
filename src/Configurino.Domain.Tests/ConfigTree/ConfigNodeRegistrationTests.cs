using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.Tests;

public sealed class ConfigNodeRegistrationTests
{
    [Fact]
    public void A_first_registration_puts_every_key_in_place_with_no_value()
    {
        var node = ConfigNodeBuilder.AConfigNode().Build();

        node.RegisterKeys(
            [Any.Declare("timeout", ConfigValueKind.Integer), Any.Declare("endpoint", ConfigValueKind.String)],
            Any.Client,
            Any.At);

        var snapshot = node.Snapshot();
        Assert.Equal(2, snapshot.Keys.Count);
        Assert.All(snapshot.Keys, key => Assert.Equal(KeyStatus.Active, key.Status));
        Assert.All(snapshot.Keys, key => Assert.False(key.IsSet));
        Assert.Equal(KeyReadResult.NotSet, node.TryGetValue(Any.Key("timeout")));
    }

    [Fact]
    public void Registering_the_same_schema_again_changes_nothing_at_all()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .Build();

        var versionBefore = node.Version;

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.Integer)], Any.Client, Any.Later(60));

        // A fleet of pods redeploying unchanged must leave no trace, or the audit trail becomes noise.
        Assert.Equal(versionBefore, node.Version);
        Assert.Empty(node.DequeueDomainEvents());
    }

    [Fact]
    public void A_key_that_is_no_longer_declared_is_kept_and_marked_obsolete()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithKey("legacy", ConfigValueKind.String)
            .WithValue("legacy", ConfigValue.Of("still in use"))
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.Integer)], Any.Client, Any.Later(60));

        var legacy = KeyIn(node, "legacy");
        Assert.Equal(KeyStatus.Obsolete, legacy.Status);

        // Kept on purpose: an older deployment still running may well be reading it.
        Assert.True(legacy.IsSet);
        Assert.Equal(ConfigValue.Of("still in use"), legacy.Value);
    }

    [Fact]
    public void Declaring_an_obsolete_key_again_brings_it_back_with_the_value_it_had()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithValue("timeout", ConfigValue.Of(30L))
            .Build();

        node.RegisterKeys([Any.Declare("other", ConfigValueKind.String)], Any.Client, Any.Later(60));
        Assert.Equal(KeyStatus.Obsolete, KeyIn(node, "timeout").Status);

        node.RegisterKeys(
            [Any.Declare("timeout", ConfigValueKind.Integer), Any.Declare("other", ConfigValueKind.String)],
            Any.Client,
            Any.Later(120));

        var timeout = KeyIn(node, "timeout");
        Assert.Equal(KeyStatus.Active, timeout.Status);
        Assert.Equal(ConfigValue.Of(30L), timeout.Value);
    }

    [Fact]
    public void Reports_what_it_did_to_each_key()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithKey("dropped", ConfigValueKind.String)
            .Build();

        node.RegisterKeys(
            [Any.Declare("timeout", ConfigValueKind.Integer), Any.Declare("added", ConfigValueKind.Boolean)],
            Any.Client,
            Any.Later(60));

        var registered = Assert.IsType<ConfigKeysRegistered>(Assert.Single(node.DequeueDomainEvents()));
        Assert.Equal([Any.Key("added")], registered.Added);
        Assert.Equal([Any.Key("dropped")], registered.MarkedObsolete);
        Assert.Empty(registered.Reactivated);
        Assert.Empty(registered.KindChanged);
        Assert.Equal(node.Version, registered.Version);
        Assert.Equal(Any.Client, registered.Registrant);
    }

    [Fact]
    public void Refuses_a_registration_that_declares_nothing()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .Build();

        // An empty declaration would quietly obsolete the whole schema, which is a client bug every time.
        Assert.Throws<EmptyRegistrationException>(() => node.RegisterKeys([], Any.Client, Any.At));
        Assert.Equal(KeyStatus.Active, KeyIn(node, "timeout").Status);
    }

    [Fact]
    public void Refuses_a_registration_that_declares_the_same_key_twice()
    {
        var node = ConfigNodeBuilder.AConfigNode().Build();

        Assert.Throws<DuplicateKeyDeclarationException>(() => node.RegisterKeys(
            [Any.Declare("timeout", ConfigValueKind.Integer), Any.Declare("TIMEOUT", ConfigValueKind.String)],
            Any.Client,
            Any.At));

        Assert.Equal(0, node.KeyCount);
    }

    [Fact]
    public void Never_loses_a_key_however_the_schema_moves()
    {
        var node = ConfigNodeBuilder.AConfigNode().Build();

        node.RegisterKeys([Any.Declare("a", ConfigValueKind.String)], Any.Client, Any.At);
        node.RegisterKeys([Any.Declare("b", ConfigValueKind.String)], Any.Client, Any.Later(1));
        node.RegisterKeys([Any.Declare("c", ConfigValueKind.String)], Any.Client, Any.Later(2));
        node.RegisterKeys([Any.Declare("a", ConfigValueKind.String)], Any.Client, Any.Later(3));

        Assert.Equal(3, node.KeyCount);
    }

    [Fact]
    public void Records_the_client_that_registered_against_every_key_it_touched()
    {
        var node = ConfigNodeBuilder.AConfigNode().Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.Integer)], Any.Client, Any.At);

        var stamp = KeyIn(node, "timeout").LastChange;
        Assert.Equal(Any.Client, stamp.By);
        Assert.Equal(Any.At, stamp.At);
        Assert.Equal(node.Version, stamp.Version);
    }

    private static ConfigKeyView KeyIn(ConfigNode node, string name) =>
        node.Snapshot().Keys.Single(key => key.Name == Any.Key(name));
}
