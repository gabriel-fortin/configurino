using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Tests;

public sealed class ConfigValueTests
{
    [Fact]
    public void Carries_the_kind_it_was_made_with()
    {
        Assert.Equal(ConfigValueKind.String, ConfigValue.Of("x").Kind);
        Assert.Equal(ConfigValueKind.Integer, ConfigValue.Of(30L).Kind);
        Assert.Equal(ConfigValueKind.Boolean, ConfigValue.Of(true).Kind);
    }

    [Fact]
    public void Compares_equal_only_within_the_same_kind()
    {
        Assert.Equal(ConfigValue.Of(30L), ConfigValue.Of(30L));
        Assert.NotEqual<ConfigValue>(ConfigValue.Of(30L), ConfigValue.Of("30"));
    }

    [Fact]
    public void Holds_whole_numbers_too_large_for_thirty_two_bits()
    {
        var epochMilliseconds = 1_800_000_000_000L;

        Assert.Equal(epochMilliseconds, ConfigValue.Of(epochMilliseconds).Value);
    }

    [Fact]
    public void Keeps_text_exactly_as_given()
    {
        Assert.Equal("  Mixed Case  ", ConfigValue.Of("  Mixed Case  ").Value);
    }
}
