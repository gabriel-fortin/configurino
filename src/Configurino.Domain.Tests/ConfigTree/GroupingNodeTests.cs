using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Tests;

public sealed class GroupingNodeTests
{
    [Fact]
    public void The_root_has_a_fixed_identity_so_creating_it_twice_yields_the_same_node()
    {
        Assert.Equal(NodeId.Root, Any.Root().Id);
        Assert.Equal(Any.Root().Id, Any.Root().Id);
    }

    [Fact]
    public void The_root_sits_under_nothing_and_has_no_name()
    {
        var root = Any.Root();

        Assert.True(root.IsRoot);
        Assert.Null(root.ParentId);
        Assert.Null(root.Name);
    }

    [Fact]
    public void A_group_can_hold_groups_and_config_nodes_alike()
    {
        var root = Any.Root();

        var team = root.CreateChildGroup(Any.Name("team-a"), Any.Human, Any.At);
        var service = root.CreateChildConfig(Any.Name("billing-api"), Any.Human, Any.At);

        Assert.Equal(root.Id, team.ParentId);
        Assert.Equal(root.Id, service.ParentId);
    }

    [Fact]
    public void Children_get_identities_of_their_own()
    {
        var root = Any.Root();

        var first = root.CreateChildGroup(Any.Name("team-a"), Any.Human, Any.At);
        var second = root.CreateChildGroup(Any.Name("team-b"), Any.Human, Any.At);

        Assert.NotEqual(first.Id, second.Id);
        Assert.NotEqual(NodeId.Root, first.Id);
    }

    [Fact]
    public void Creating_a_node_announces_it()
    {
        var root = Any.Root();
        root.DequeueDomainEvents();

        var team = root.CreateChildGroup(Any.Name("team-a"), Any.Human, Any.At);

        var created = Assert.IsType<GroupingNodeCreated>(Assert.Single(team.DequeueDomainEvents()));
        Assert.Equal(team.Id, created.Id);
        Assert.Equal(root.Id, created.ParentId);
        Assert.Equal(Any.Name("team-a"), created.Name);
        Assert.Equal(Any.Human, created.By);

        // The parent is untouched: creating a child writes one aggregate, not two.
        Assert.Empty(root.DequeueDomainEvents());
    }

    [Fact]
    public void Creating_a_config_node_announces_it_as_a_config_node()
    {
        var service = Any.Root().CreateChildConfig(Any.Name("billing-api"), Any.Human, Any.At);

        var created = Assert.IsType<ConfigNodeCreated>(Assert.Single(service.DequeueDomainEvents()));
        Assert.Equal(service.Id, created.Id);
        Assert.Equal(Any.Name("billing-api"), created.Name);
    }

    [Fact]
    public void A_config_node_offers_no_way_to_put_anything_underneath_it()
    {
        var service = Any.Root().CreateChildConfig(Any.Name("billing-api"), Any.Human, Any.At);

        // Not a runtime rule to enforce but a fact about the type: ConfigNode has no child factory at all,
        // which is why nothing below can even be written.
        var childFactories = typeof(ConfigNode)
            .GetMethods()
            .Where(method => method.Name.StartsWith("CreateChild", StringComparison.Ordinal));

        Assert.Empty(childFactories);
        Assert.Equal(NodeId.Root, service.ParentId);
    }

    [Fact]
    public void Refuses_a_timestamp_that_is_not_utc()
    {
        var localTime = new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<ArgumentException>(() => Any.Root().CreateChildGroup(Any.Name("team-a"), Any.Human, localTime));
    }

    [Fact]
    public void Handing_over_the_events_empties_them()
    {
        var root = Any.Root();

        Assert.Single(root.DequeueDomainEvents());
        Assert.Empty(root.DequeueDomainEvents());
    }
}
