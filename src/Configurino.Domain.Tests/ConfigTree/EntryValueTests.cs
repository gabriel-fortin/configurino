using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.Tests;

public sealed class EntryValueTests
{
    [Fact]
    public void Carries_the_kind_it_was_made_with()
    {
        Assert.Equal(ValueKind.String, EntryValue.Of("x").Kind);
        Assert.Equal(ValueKind.Integer, EntryValue.Of(30L).Kind);
        Assert.Equal(ValueKind.Boolean, EntryValue.Of(true).Kind);
    }

    [Fact]
    public void Compares_equal_only_within_the_same_kind()
    {
        Assert.Equal(EntryValue.Of(30L), EntryValue.Of(30L));
        Assert.NotEqual<EntryValue>(EntryValue.Of(30L), EntryValue.Of("30"));
    }

    [Fact]
    public void Holds_whole_numbers_too_large_for_thirty_two_bits()
    {
        var epochMilliseconds = 1_800_000_000_000L;

        Assert.Equal(epochMilliseconds, EntryValue.Of(epochMilliseconds).Value);
    }

    [Fact]
    public void Keeps_text_exactly_as_given()
    {
        Assert.Equal("  Mixed Case  ", EntryValue.Of("  Mixed Case  ").Value);
    }
}
