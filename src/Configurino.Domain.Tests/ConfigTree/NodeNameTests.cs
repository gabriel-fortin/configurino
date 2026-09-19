using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.Tests;

public sealed class NodeNameTests
{
    [Theory]
    [InlineData("billing")]
    [InlineData("billing-api")]
    [InlineData("billing_api")]
    [InlineData("v2")]
    [InlineData("2nd")]
    public void Accepts_ordinary_names(string value) => Assert.Equal(value, NodeName.Parse(value).Value);

    [Fact]
    public void Lower_cases_what_it_is_given()
    {
        Assert.Equal("billing", NodeName.Parse("Billing").Value);
    }

    [Fact]
    public void Treats_names_differing_only_by_case_as_the_same_name()
    {
        Assert.Equal(NodeName.Parse("Billing"), NodeName.Parse("bILLing"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("-leading-dash")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("has space")]
    [InlineData("has/separator")]
    [InlineData("has:colon")]
    public void Rejects_names_that_break_the_rules(string? value)
    {
        Assert.Throws<InvalidNodeNameException>(() => NodeName.Parse(value));
    }

    [Fact]
    public void Rejects_a_name_longer_than_the_limit()
    {
        var tooLong = new string('a', NodeName.MaxLength + 1);

        Assert.Throws<InvalidNodeNameException>(() => NodeName.Parse(tooLong));
    }

    [Fact]
    public void Reports_failure_without_throwing_when_asked_to_try()
    {
        Assert.False(NodeName.TryParse("has space", out var name));
        Assert.Null(name);
    }
}
