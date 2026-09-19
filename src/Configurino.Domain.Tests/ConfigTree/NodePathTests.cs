using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.Tests;

public sealed class NodePathTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/prod")]
    [InlineData("/prod/billing")]
    public void Survives_a_round_trip_through_text(string value)
    {
        Assert.Equal(value, NodePath.Parse(value).ToString());
    }

    [Fact]
    public void Reads_the_root_as_having_no_segments()
    {
        var root = NodePath.Parse("/");

        Assert.True(root.IsRoot);
        Assert.Empty(root.Segments);
        Assert.Null(root.Name);
        Assert.Null(root.Parent);
    }

    [Fact]
    public void Names_the_last_segment_and_points_at_the_one_above()
    {
        var path = NodePath.Parse("/prod/billing");

        Assert.Equal(Any.Name("billing"), path.Name);
        Assert.Equal(NodePath.Parse("/prod"), path.Parent);
        Assert.Equal(2, path.Depth);
    }

    [Fact]
    public void Walks_all_the_way_up_to_the_root()
    {
        var path = NodePath.Parse("/prod/team-a/billing");

        Assert.Equal(NodePath.Root, path.Parent?.Parent?.Parent);
    }

    [Fact]
    public void Appending_a_name_goes_one_level_deeper()
    {
        var path = NodePath.Parse("/prod").Append(Any.Name("billing"));

        Assert.Equal("/prod/billing", path.Value);
    }

    [Fact]
    public void Compares_by_what_it_addresses_rather_than_by_instance()
    {
        Assert.Equal(NodePath.Parse("/prod/billing"), NodePath.Parse("/PROD/Billing"));
        Assert.NotEqual(NodePath.Parse("/prod/billing"), NodePath.Parse("/prod/search"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("prod")]
    [InlineData("/prod/")]
    [InlineData("//prod")]
    [InlineData("/prod//billing")]
    [InlineData("/prod/has space")]
    public void Rejects_paths_that_break_the_rules(string? value)
    {
        Assert.Throws<InvalidNodePathException>(() => NodePath.Parse(value));
    }

    [Fact]
    public void Refuses_to_go_deeper_than_the_limit()
    {
        var deepest = Enumerable
            .Range(0, NodePath.MaxDepth)
            .Aggregate(NodePath.Root, (path, level) => path.Append(Any.Name($"level{level}")));

        Assert.Throws<InvalidNodePathException>(() => deepest.Append(Any.Name("toofar")));
    }
}
