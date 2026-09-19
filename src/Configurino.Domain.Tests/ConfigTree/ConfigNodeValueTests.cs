using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Domain.Tests;

public sealed class ConfigNodeValueTests
{
    [Fact]
    public void A_value_can_be_set_and_read_back()
    {
        var node = Node();

        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());

        Assert.Equal(KeyReadResult.Set(EntryValue.Of(30L)), node.TryGetValue(Any.Key("timeout")));
    }

    [Fact]
    public void A_key_nobody_registered_reads_differently_from_one_nobody_filled_in()
    {
        var node = Node();

        Assert.Equal(KeyReadResult.NotSet, node.TryGetValue(Any.Key("timeout")));
        Assert.Equal(KeyReadResult.Unknown, node.TryGetValue(Any.Key("never-heard-of-it")));
    }

    [Fact]
    public void Refuses_a_value_of_the_wrong_type()
    {
        var node = Node();

        var mismatch = Assert.Throws<ValueKindMismatchException>(() => node.SetValue(
            Any.Key("timeout"),
            EntryValue.Of("half a minute"),
            Any.Human,
            Any.At,
            ChangeReason.Manual()));

        Assert.Equal(ValueKind.Integer, mismatch.Expected);
        Assert.Equal(ValueKind.String, mismatch.Actual);
    }

    [Fact]
    public void Refuses_a_key_that_was_never_registered()
    {
        var node = Node();

        Assert.Throws<UnknownConfigKeyException>(() => node.SetValue(
            Any.Key("invented"),
            EntryValue.Of("x"),
            Any.Human,
            Any.At,
            ChangeReason.Manual()));
    }

    [Fact]
    public void Refuses_a_key_no_client_declares_any_more()
    {
        var node = Node();
        node.RegisterKeys([Any.Declare("other", ValueKind.String)], Any.Client, Any.Later(1));

        Assert.Throws<ObsoleteKeyException>(() => node.SetValue(
            Any.Key("timeout"),
            EntryValue.Of(30L),
            Any.Human,
            Any.At,
            ChangeReason.Manual()));
    }

    [Fact]
    public void Applies_nothing_at_all_when_one_assignment_in_a_batch_is_wrong()
    {
        var node = Node();

        Assert.Throws<ValueKindMismatchException>(() => node.SetValues(
            [
                new KeyAssignment(Any.Key("timeout"), EntryValue.Of(30L)),
                new KeyAssignment(Any.Key("endpoint"), EntryValue.Of(true)),
            ],
            Any.Human,
            Any.At,
            ChangeReason.Manual()));

        Assert.Equal(KeyReadResult.NotSet, node.TryGetValue(Any.Key("timeout")));
        Assert.Empty(node.DequeueDomainEvents());
    }

    [Fact]
    public void Refuses_the_same_key_twice_in_one_change_set()
    {
        var node = Node();

        Assert.Throws<DuplicateAssignmentException>(() => node.SetValues(
            [
                new KeyAssignment(Any.Key("timeout"), EntryValue.Of(30L)),
                new KeyAssignment(Any.Key("timeout"), EntryValue.Of(60L)),
            ],
            Any.Human,
            Any.At,
            ChangeReason.Manual()));
    }

    [Fact]
    public void Setting_a_value_to_what_it_already_is_does_nothing()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());
        node.DequeueDomainEvents();

        var versionBefore = node.Version;
        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.Later(5), ChangeReason.Manual());

        // Pressing save without editing anything must not fill the history with entries saying nothing.
        Assert.Equal(versionBefore, node.Version);
        Assert.Empty(node.DequeueDomainEvents());
    }

    [Fact]
    public void Everything_changed_together_travels_as_one_change()
    {
        var node = Node();

        node.SetValues(
            [
                new KeyAssignment(Any.Key("timeout"), EntryValue.Of(30L)),
                new KeyAssignment(Any.Key("endpoint"), EntryValue.Of("https://example")),
            ],
            Any.Human,
            Any.At,
            ChangeReason.Manual("initial setup"));

        var changed = Assert.IsType<ConfigValuesChanged>(Assert.Single(node.DequeueDomainEvents()));
        Assert.Equal(2, changed.Changes.Count);
        Assert.Equal(node.Version, changed.Version);
        Assert.Equal(Any.Human, changed.By);
        Assert.Equal(ChangeReason.Manual("initial setup"), changed.Reason);
    }

    [Fact]
    public void Only_the_keys_that_actually_moved_are_recorded()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());
        node.DequeueDomainEvents();

        node.SetValues(
            [
                new KeyAssignment(Any.Key("timeout"), EntryValue.Of(30L)),
                new KeyAssignment(Any.Key("endpoint"), EntryValue.Of("https://example")),
            ],
            Any.Human,
            Any.Later(5),
            ChangeReason.Manual());

        var changed = Assert.IsType<ConfigValuesChanged>(Assert.Single(node.DequeueDomainEvents()));
        var change = Assert.Single(changed.Changes);
        Assert.Equal(Any.Key("endpoint"), change.Key);
    }

    [Fact]
    public void A_value_can_be_taken_back_off_a_key()
    {
        var node = Node();
        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.At, ChangeReason.Manual());
        node.DequeueDomainEvents();

        node.ClearValues([Any.Key("timeout")], Any.Human, Any.Later(5), ChangeReason.Manual("set by mistake"));

        Assert.Equal(KeyReadResult.NotSet, node.TryGetValue(Any.Key("timeout")));

        var changed = Assert.IsType<ConfigValuesChanged>(Assert.Single(node.DequeueDomainEvents()));
        var change = Assert.Single(changed.Changes);
        Assert.Equal(EntryValue.Of(30L), change.Previous);
        Assert.Null(change.Current);
    }

    [Fact]
    public void Clearing_a_key_that_holds_nothing_does_nothing()
    {
        var node = Node();

        node.ClearValues([Any.Key("timeout")], Any.Human, Any.At, ChangeReason.Manual());

        Assert.Empty(node.DequeueDomainEvents());
    }

    [Fact]
    public void Records_who_changed_each_key_and_when()
    {
        var node = Node();

        node.SetValue(Any.Key("timeout"), EntryValue.Of(30L), Any.Human, Any.Later(5), ChangeReason.Manual());

        var stamp = node.Snapshot().Keys.Single(key => key.Name == Any.Key("timeout")).LastChange;
        Assert.Equal(Any.Human, stamp.By);
        Assert.Equal(Any.Later(5), stamp.At);
        Assert.Equal(node.Version, stamp.Version);
    }

    [Fact]
    public void Refuses_a_timestamp_that_is_not_utc()
    {
        var node = Node();
        var localTime = new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<ArgumentException>(() => node.SetValue(
            Any.Key("timeout"),
            EntryValue.Of(30L),
            Any.Human,
            localTime,
            ChangeReason.Manual()));
    }

    private static ConfigNode Node() => ConfigNodeBuilder.AConfigNode()
        .WithKey("timeout", ValueKind.Integer)
        .WithKey("endpoint", ValueKind.String)
        .Build();
}
