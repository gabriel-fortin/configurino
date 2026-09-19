using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Tests;

public sealed class ConfigNodeSnapshotTests
{
    [Fact]
    public void Shows_obsolete_keys_too_rather_than_hiding_them()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .WithKey("legacy", ConfigValueKind.String)
            .WithValue("legacy", ConfigValue.Of("still read by v1"))
            .Build();

        node.RegisterKeys([Any.Declare("timeout", ConfigValueKind.Integer)], Any.Client, Any.Later(60));

        var snapshot = node.Snapshot();

        // An older deployment still running reads this key; leaving it out of the snapshot would break the
        // compatibility that keeping obsolete keys exists to give.
        var legacy = snapshot.Keys.Single(key => key.Name == Any.Key("legacy"));
        Assert.Equal(KeyStatus.Obsolete, legacy.Status);
        Assert.Equal(ConfigValue.Of("still read by v1"), legacy.Value);

        Assert.Equal(2, snapshot.Keys.Count);
        Assert.Single(snapshot.ActiveKeys);
    }

    [Fact]
    public void Carries_the_version_the_configuration_was_read_at()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("timeout", ConfigValueKind.Integer)
            .Build();

        var before = node.Snapshot().Version;

        node.SetValue(Any.Key("timeout"), ConfigValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());

        // This is what a client application sends back to ask whether anything has changed since.
        Assert.Equal(before + 1, node.Snapshot().Version);
        Assert.Equal(node.Version, node.Snapshot().Version);
    }

    [Fact]
    public void Lists_keys_in_a_stable_order()
    {
        var node = ConfigNodeBuilder.AConfigNode()
            .WithKey("zulu", ConfigValueKind.String)
            .WithKey("alpha", ConfigValueKind.String)
            .WithKey("mike", ConfigValueKind.String)
            .Build();

        Assert.Equal(
            [Any.Key("alpha"), Any.Key("mike"), Any.Key("zulu")],
            node.Snapshot().Keys.Select(key => key.Name));
    }

    [Fact]
    public void Identifies_the_node_it_came_from()
    {
        var node = ConfigNodeBuilder.AConfigNode().Build();

        Assert.Equal(node.Id, node.Snapshot().NodeId);
    }
}
