using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Application.Tree.CreateConfigNode;
using org.g14.Configurino.Application.Tree.CreateGroupingNode;
using org.g14.Configurino.Application.Tree.EnsureRoot;
using org.g14.Configurino.Application.Tree.GetChildren;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Application.Tests.Tree;

public class TreeUseCaseTests
{
    [Fact]
    public async Task EnsureRoot_creates_the_root_once_however_often_it_runs()
    {
        var world = new World();

        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Human));
        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Human));

        Assert.Equal(NodeId.Root, await world.Store.Resolve(NodePath.Root));
    }

    [Fact]
    public async Task CreateGroupingNode_returns_the_new_node_and_its_path()
    {
        var world = new World();
        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Human));

        var created = await world.CreateGroupingNode.Handle(
            new CreateGroupingNodeCommand(NodePath.Root, World.Name("prod"), World.Human));

        Assert.Equal(World.Path("/prod"), created.Path);
        Assert.Equal(created.Id, await world.Store.Resolve(World.Path("/prod")));
    }

    [Fact]
    public async Task CreateConfigNode_creates_a_config_node_under_the_group()
    {
        var world = await World.WithBillingNode();

        var id = await world.Store.Resolve(World.Billing);

        Assert.NotNull(id);
        Assert.Equal(NodeKind.Config, await world.Store.GetKind(id));
    }

    [Fact]
    public async Task A_name_cannot_be_used_twice_under_one_parent_whatever_the_kind()
    {
        var world = await World.WithBillingNode();

        await Assert.ThrowsAsync<NodeNameAlreadyUsedException>(() =>
            world.CreateGroupingNode.Handle(
                new CreateGroupingNodeCommand(World.Path("/prod"), World.Name("billing"), World.Human)));
    }

    [Fact]
    public async Task Nothing_can_be_created_under_a_config_node()
    {
        var world = await World.WithBillingNode();

        var thrown = await Assert.ThrowsAsync<NodeKindMismatchException>(() =>
            world.CreateConfigNode.Handle(
                new CreateConfigNodeCommand(World.Billing, World.Name("child"), World.Human)));

        Assert.Equal(NodeKind.Grouping, thrown.Expected);
        Assert.Equal(NodeKind.Config, thrown.Actual);
    }

    [Fact]
    public async Task Creating_under_a_missing_parent_says_the_parent_is_missing()
    {
        var world = new World();
        await world.EnsureRoot.Handle(new EnsureRootCommand(World.Human));

        await Assert.ThrowsAsync<GroupingNodeNotFoundException>(() =>
            world.CreateGroupingNode.Handle(
                new CreateGroupingNodeCommand(World.Path("/nowhere"), World.Name("x"), World.Human)));
    }

    [Fact]
    public async Task GetChildren_lists_what_sits_directly_under_a_group_ordered_by_name()
    {
        var world = await World.WithBillingNode();
        await world.CreateGroupingNode.Handle(
            new CreateGroupingNodeCommand(World.Path("/prod"), World.Name("analytics"), World.Human));

        var children = await world.GetChildren.Handle(new GetChildrenQuery(World.Path("/prod")));

        Assert.Equal(["analytics", "billing"], children.Select(child => child.Name.Value));
        Assert.Equal([NodeKind.Grouping, NodeKind.Config], children.Select(child => child.Kind));
    }

    [Fact]
    public async Task GetChildren_of_a_config_node_is_refused()
    {
        var world = await World.WithBillingNode();

        await Assert.ThrowsAsync<NodeKindMismatchException>(() =>
            world.GetChildren.Handle(new GetChildrenQuery(World.Billing)));
    }
}
