using org.g14.Configurino.Application.Config.ClearValues;
using org.g14.Configurino.Application.Config.GetConfigSnapshot;
using org.g14.Configurino.Application.Config.GetValue;
using org.g14.Configurino.Application.Config.RegisterKeys;
using org.g14.Configurino.Application.Config.Rollback;
using org.g14.Configurino.Application.Config.SetValues;
using org.g14.Configurino.Application.History;
using org.g14.Configurino.Application.History.GetChangeHistory;
using org.g14.Configurino.Application.History.GetChangeSet;
using org.g14.Configurino.Application.Tree.CreateConfigNode;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Application.Tests.Config;

public class ValueAndRollbackTests
{
    private static async Task<World> WithKeys()
    {
        var world = await World.WithBillingNode();

        await world.RegisterKeys.Handle(
            new RegisterKeysCommand(
                World.Billing,
                [World.Declare("host", ValueKind.String), World.Declare("mode", ValueKind.String)],
                World.Client));

        return world;
    }

    private static Task<int> Set(World world, string key, string value) =>
        world.SetValues.Handle(new SetValuesCommand(World.Billing, [World.Assign(key, value)], World.Human));

    private static async Task<ChangeSetId> LatestChangeSet(World world) =>
        (await world.GetChangeHistory.Handle(new GetChangeHistoryQuery(World.Billing, 1)))[0].Id;

    [Fact]
    public async Task A_set_value_can_be_read_back()
    {
        var world = await WithKeys();

        await Set(world, "host", "db1");

        var read = await world.GetValue.Handle(new GetValueQuery(World.Billing, World.Key("host")));
        Assert.Equal(KeyReadResult.Set(EntryValue.Of("db1")), read);
    }

    [Fact]
    public async Task Setting_the_value_it_already_has_saves_nothing()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");
        var saves = world.Store.SaveCalls;

        await Set(world, "host", "db1");

        Assert.Equal(saves, world.Store.SaveCalls);
    }

    [Fact]
    public async Task Clearing_a_value_makes_it_read_as_not_set()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");

        await world.ClearValues.Handle(new ClearValuesCommand(World.Billing, [World.Key("host")], World.Human));

        var read = await world.GetValue.Handle(new GetValueQuery(World.Billing, World.Key("host")));
        Assert.Equal(KeyReadResult.NotSet, read);
    }

    [Fact]
    public async Task Rollback_puts_the_previous_value_back_and_records_why()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");
        await Set(world, "host", "db2");
        var target = await LatestChangeSet(world);

        await world.Rollback.Handle(new RollbackCommand(World.Billing, target, World.Human));

        var read = await world.GetValue.Handle(new GetValueQuery(World.Billing, World.Key("host")));
        Assert.Equal(KeyReadResult.Set(EntryValue.Of("db1")), read);
        Assert.Equal(ChangeReason.Rollback(target), (await world.Store.GetChanges(
            (await world.Store.Resolve(World.Billing))!, 1))[0].Reason);
    }

    [Fact]
    public async Task Rolling_back_a_first_assignment_clears_the_key()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");

        await world.Rollback.Handle(
            new RollbackCommand(World.Billing, await LatestChangeSet(world), World.Human));

        var read = await world.GetValue.Handle(new GetValueQuery(World.Billing, World.Key("host")));
        Assert.Equal(KeyReadResult.NotSet, read);
    }

    [Fact]
    public async Task Rollback_is_refused_when_a_later_edit_touched_the_same_key()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");
        var target = await LatestChangeSet(world);
        await Set(world, "host", "db2");

        var thrown = await Assert.ThrowsAsync<RollbackConflictException>(() =>
            world.Rollback.Handle(new RollbackCommand(World.Billing, target, World.Human)));

        Assert.Equal([World.Key("host")], thrown.Keys);
    }

    [Fact]
    public async Task A_registration_cannot_be_rolled_back()
    {
        var world = await WithKeys();
        var registration = await LatestChangeSet(world);

        await Assert.ThrowsAsync<ChangeSetNotRollbackableException>(() =>
            world.Rollback.Handle(new RollbackCommand(World.Billing, registration, World.Human)));
    }

    [Fact]
    public async Task A_change_set_of_another_node_is_not_found()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");
        var target = await LatestChangeSet(world);
        await world.CreateConfigNode.Handle(
            new CreateConfigNodeCommand(World.Path("/prod"), World.Name("other"), World.Human));

        await Assert.ThrowsAsync<ChangeSetNotFoundException>(() =>
            world.Rollback.Handle(new RollbackCommand(World.Path("/prod/other"), target, World.Human)));
        await Assert.ThrowsAsync<ChangeSetNotFoundException>(() =>
            world.GetChangeSet.Handle(new GetChangeSetQuery(World.Path("/prod/other"), target)));
    }

    [Fact]
    public async Task Snapshot_is_not_modified_when_the_caller_already_has_the_current_version()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");

        var first = await world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Billing));
        var changed = Assert.IsType<SnapshotResult.Changed>(first);

        var again = await world.GetConfigSnapshot.Handle(
            new GetConfigSnapshotQuery(World.Billing, changed.Snapshot.Version));

        Assert.Equal(new SnapshotResult.NotModified(changed.Snapshot.Version), again);
    }

    [Fact]
    public async Task History_is_paged_newest_first()
    {
        var world = await WithKeys();
        await Set(world, "host", "db1");
        await Set(world, "host", "db2");

        var firstPage = await world.GetChangeHistory.Handle(new GetChangeHistoryQuery(World.Billing, 1));
        var secondPage = await world.GetChangeHistory.Handle(
            new GetChangeHistoryQuery(World.Billing, 5, firstPage[0].Id));

        Assert.Equal(2, secondPage.Count); // the first assignment and the registration
        Assert.True(firstPage[0].NodeVersion > secondPage[0].NodeVersion);
    }
}
