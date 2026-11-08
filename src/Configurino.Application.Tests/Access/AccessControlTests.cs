using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Access.GetAccessList;
using org.g14.Configurino.Application.Access.GetEffectiveRights;
using org.g14.Configurino.Application.Access.SetRole;
using org.g14.Configurino.Application.Config.ClearValues;
using org.g14.Configurino.Application.Config.GetConfigSnapshot;
using org.g14.Configurino.Application.Config.GetValue;
using org.g14.Configurino.Application.Config.RegisterKeys;
using org.g14.Configurino.Application.Config.SetValues;
using org.g14.Configurino.Application.History.GetChangeHistory;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Application.Tree.CreateConfigNode;
using org.g14.Configurino.Application.Tree.CreateGroupingNode;
using org.g14.Configurino.Application.Tree.EnsureRoot;
using org.g14.Configurino.Application.Tree.GetChildren;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.Access.Events;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Tests.Access;

public class AccessControlTests
{
    private static readonly NodePath Prod = World.Path("/prod");

    private static async Task<World> WithKeys()
    {
        var world = await World.WithBillingNode();

        await world.RegisterKeys.Handle(
            new RegisterKeysCommand(World.Billing, [World.Declare("host", ValueKind.String)], World.Client));

        return world;
    }

    private static Task<int> SetRole(World world, NodePath path, ActorId actor, Role role) =>
        world.SetRole.Handle(new SetRoleCommand(path, actor, role, World.Human));

    private static Task<int> SetHost(World world, ActorId by) =>
        world.SetValues.Handle(new SetValuesCommand(World.Billing, [World.Assign("host", "db1")], by));

    /// <summary>Every operation a stranger tries on the billing node or its parent, by name.</summary>
    private static readonly Dictionary<string, (NodeRight Required, Func<World, Task> Attempt)> StrangerAttempts = new()
    {
        ["GetChildren"] = (NodeRight.Browse, world =>
            world.GetChildren.Handle(new GetChildrenQuery(Prod, World.Stranger))),
        ["CreateGroupingNode"] = (NodeRight.CreateNodes, world =>
            world.CreateGroupingNode.Handle(new CreateGroupingNodeCommand(Prod, World.Name("x"), World.Stranger))),
        ["CreateConfigNode"] = (NodeRight.CreateNodes, world =>
            world.CreateConfigNode.Handle(new CreateConfigNodeCommand(Prod, World.Name("x"), World.Stranger))),
        ["RegisterKeys"] = (NodeRight.RegisterKeys, world =>
            world.RegisterKeys.Handle(
                new RegisterKeysCommand(World.Billing, [World.Declare("x", ValueKind.String)], World.Stranger))),
        ["GetConfigSnapshot"] = (NodeRight.ReadConfig, world =>
            world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Billing, World.Stranger))),
        ["GetValue"] = (NodeRight.ReadConfig, world =>
            world.GetValue.Handle(new GetValueQuery(World.Billing, World.Key("host"), World.Stranger))),
        ["SetValues"] = (NodeRight.EditValues, world => SetHost(world, World.Stranger)),
        ["ClearValues"] = (NodeRight.EditValues, world =>
            world.ClearValues.Handle(new ClearValuesCommand(World.Billing, [World.Key("host")], World.Stranger))),
        ["GetChangeHistory"] = (NodeRight.ReadHistory, world =>
            world.GetChangeHistory.Handle(new GetChangeHistoryQuery(World.Billing, World.Stranger, 10))),
        ["SetRole"] = (NodeRight.ManagePermissions, world =>
            world.SetRole.Handle(new SetRoleCommand(World.Billing, World.Client, Role.NoAccess, World.Stranger))),
        ["GetAccessList"] = (NodeRight.ManagePermissions, world =>
            world.GetAccessList.Handle(new GetAccessListQuery(World.Billing, World.Stranger))),
        ["GetEffectiveRights"] = (NodeRight.Browse, world =>
            world.GetEffectiveRights.Handle(new GetEffectiveRightsQuery(World.Billing, World.Stranger))),
    };

    public static TheoryData<string> Operations => [.. StrangerAttempts.Keys];

    /// <summary>Every operation but registering keys, which is a client application's job, not an owner's.</summary>
    public static TheoryData<string> OwnerOperations => [.. StrangerAttempts.Keys.Where(name => name != "RegisterKeys")];

    // --- bootstrapping --------------------------------------------------------------------------

    [Fact]
    public async Task Anyone_may_create_the_root_and_the_first_to_do_so_owns_it()
    {
        var world = new World();

        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Stranger));

        Assert.Equal(NodeId.Root, await world.Store.Resolve(NodePath.Root));
        Assert.Equal(Role.Owner.Rights, await world.Guard.EffectiveRights(World.Stranger, NodeId.Root));
    }

    [Fact]
    public async Task The_first_user_owns_the_whole_tree()
    {
        var world = await World.WithBillingNode();

        var rights = await world.GetEffectiveRights.Handle(new GetEffectiveRightsQuery(World.Billing, World.Human));

        Assert.Equal(Role.Owner.Rights, rights);
    }

    [Fact]
    public async Task Ensuring_the_root_again_gives_a_later_user_nothing()
    {
        var world = await World.WithRoot();

        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Stranger));

        Assert.Equal(NodeRight.None, await world.Guard.EffectiveRights(World.Stranger, NodeId.Root));
        Assert.Equal(Role.Owner.Rights, await world.Guard.EffectiveRights(World.Human, NodeId.Root));
    }

    [Fact]
    public async Task A_root_left_without_an_access_list_is_owned_by_the_next_user()
    {
        var world = new World();
        await world.Store.Add(GroupingNode.CreateRoot(World.Stranger, World.Start));

        await world.EnsureRoot.Handle(World.StartUp);

        Assert.Equal(Role.Owner.Rights, await world.Guard.EffectiveRights(World.Human, NodeId.Root));
    }

    // --- enforcement ----------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task An_actor_without_a_permission_is_denied(string operation)
    {
        var world = await WithKeys();
        var (required, attempt) = StrangerAttempts[operation];

        var thrown = await Assert.ThrowsAsync<AccessDeniedException>(() => attempt(world));

        Assert.Equal(World.Stranger, thrown.Actor);
        Assert.Equal(required, thrown.Required);
    }

    [Theory]
    [MemberData(nameof(OwnerOperations))]
    public async Task An_owner_of_a_node_above_may_do_everything_but_register_keys(string operation)
    {
        var world = await WithKeys();
        await SetRole(world, Prod, World.Stranger, Role.Owner);

        await StrangerAttempts[operation].Attempt(world);
    }

    [Fact]
    public async Task An_owner_may_not_register_keys()
    {
        var world = await WithKeys();
        await SetRole(world, Prod, World.Stranger, Role.Owner);

        var thrown = await Assert.ThrowsAsync<AccessDeniedException>(() => StrangerAttempts["RegisterKeys"].Attempt(world));

        Assert.Equal(NodeRight.RegisterKeys, thrown.Required);
    }

    [Fact]
    public async Task A_permission_reaches_every_node_below_it()
    {
        var world = await WithKeys();

        await SetRole(world, NodePath.Root, World.Stranger, Role.Editor);

        await SetHost(world, World.Stranger);
    }

    [Fact]
    public async Task A_permission_on_a_sibling_does_not_reach_across()
    {
        var world = await WithKeys();
        await world.CreateConfigNode.Handle(new CreateConfigNodeCommand(Prod, World.Name("other"), World.Human));

        await SetRole(world, World.Path("/prod/other"), World.Stranger, Role.Editor);

        await Assert.ThrowsAsync<AccessDeniedException>(() => SetHost(world, World.Stranger));
    }

    [Fact]
    public async Task A_client_may_register_and_read_but_not_edit()
    {
        var world = await WithKeys();

        await world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Billing, World.Client));

        await Assert.ThrowsAsync<AccessDeniedException>(() => SetHost(world, World.Client));
    }

    [Fact]
    public async Task A_missing_node_is_reported_as_missing_whoever_asks()
    {
        var world = await WithKeys();

        await Assert.ThrowsAsync<ConfigNodeNotFoundException>(() =>
            world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Path("/prod/ghost"), World.Stranger)));
        await Assert.ThrowsAsync<NodeNotFoundException>(() =>
            world.GetAccessList.Handle(new GetAccessListQuery(World.Path("/prod/ghost"), World.Stranger)));
    }

    // --- managing permissions -------------------------------------------------------------------

    [Fact]
    public async Task Setting_a_role_is_recorded_for_the_audit_trail()
    {
        var world = await WithKeys();

        await SetRole(world, Prod, World.Stranger, Role.Reader);

        var set = Assert.IsType<RoleSet>(world.Store.AllAccessChanges[^1]);
        Assert.Equal(World.Stranger, set.Actor);
        Assert.Equal(NodeRight.None, set.Previous);
        Assert.Equal(Role.Reader.Rights, set.Current);
        Assert.Equal(World.Human, set.By);
    }

    [Fact]
    public async Task Setting_NoAccess_takes_the_permission_away()
    {
        var world = await WithKeys();
        await SetRole(world, World.Billing, World.Stranger, Role.Editor);

        await SetRole(world, World.Billing, World.Stranger, Role.NoAccess);

        await Assert.ThrowsAsync<AccessDeniedException>(() =>
            world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Billing, World.Stranger)));
    }

    [Fact]
    public async Task Setting_a_narrower_role_takes_away_what_it_lacks()
    {
        var world = await WithKeys();
        await SetRole(world, World.Billing, World.Stranger, Role.Editor);

        await SetRole(world, World.Billing, World.Stranger, Role.Reader);

        await world.GetConfigSnapshot.Handle(new GetConfigSnapshotQuery(World.Billing, World.Stranger));
        await Assert.ThrowsAsync<AccessDeniedException>(() => SetHost(world, World.Stranger));
    }

    [Fact]
    public async Task NoAccess_on_a_node_leaves_what_is_inherited_from_above()
    {
        var world = await WithKeys();
        await SetRole(world, Prod, World.Stranger, Role.Editor);
        await SetRole(world, World.Billing, World.Stranger, Role.Editor);

        await SetRole(world, World.Billing, World.Stranger, Role.NoAccess);

        await SetHost(world, World.Stranger);
    }

    [Fact]
    public async Task NoAccess_where_nothing_was_set_changes_nothing()
    {
        var world = await WithKeys();

        var version = await SetRole(world, Prod, World.Stranger, Role.NoAccess);

        Assert.Equal(0, version);
    }

    [Fact]
    public async Task The_access_list_shows_own_permissions_and_where_inherited_ones_come_from()
    {
        var world = await WithKeys();
        await SetRole(world, Prod, World.Stranger, Role.Reader);

        var view = await world.GetAccessList.Handle(new GetAccessListQuery(World.Billing, World.Human));

        Assert.Equal([new Permission(World.Client, Role.ClientApp.Rights)], view.Own);
        Assert.Equal(
            [
                new InheritedPermission(NodePath.Root, new Permission(World.Human, Role.Owner.Rights)),
                new InheritedPermission(Prod, new Permission(World.Stranger, Role.Reader.Rights)),
            ],
            view.Inherited);
    }
}
