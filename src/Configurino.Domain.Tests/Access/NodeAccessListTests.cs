using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.Access.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Tests;

public sealed class NodeAccessListTests
{
    private static readonly ActorId Admin = ActorId.Parse("admin");

    private static NodeAccessList EmptyList() => NodeAccessList.For(NodeId.New());

    [Fact]
    public void A_new_list_gives_nobody_anything()
    {
        var list = EmptyList();

        Assert.Empty(list.Permissions);
        Assert.Equal(NodeRight.None, list.RightsOf(Any.Human));
        Assert.Equal(0, list.Version);
    }

    [Fact]
    public void Setting_a_role_gives_exactly_its_rights()
    {
        var list = EmptyList();

        list.SetRole(Any.Human, Role.Editor, Admin, Any.At);

        Assert.Equal(Role.Editor.Rights, list.RightsOf(Any.Human));
        Assert.Equal([new Permission(Any.Human, Role.Editor.Rights)], list.Permissions);
    }

    [Fact]
    public void Setting_a_narrower_role_replaces_the_wider_one()
    {
        var list = EmptyList();
        list.SetRole(Any.Human, Role.Editor, Admin, Any.At);

        list.SetRole(Any.Human, Role.Reader, Admin, Any.Later(1));

        Assert.Equal(Role.Reader.Rights, list.RightsOf(Any.Human));
    }

    [Fact]
    public void Setting_the_role_already_held_changes_nothing()
    {
        var list = EmptyList();
        list.SetRole(Any.Human, Role.Editor, Admin, Any.At);
        list.DequeueDomainEvents();
        var version = list.Version;

        list.SetRole(Any.Human, Role.Editor, Admin, Any.Later(1));

        Assert.Equal(version, list.Version);
        Assert.Empty(list.DequeueDomainEvents());
    }

    [Fact]
    public void Setting_a_role_announces_what_was_held_before_and_what_is_held_now()
    {
        var list = EmptyList();
        list.SetRole(Any.Human, Role.Editor, Admin, Any.At);
        list.DequeueDomainEvents();

        list.SetRole(Any.Human, Role.Reader, Admin, Any.Later(1));

        var set = Assert.IsType<RoleSet>(Assert.Single(list.DequeueDomainEvents()));
        Assert.Equal(list.NodeId, set.NodeId);
        Assert.Equal(Any.Human, set.Actor);
        Assert.Equal(Role.Editor.Rights, set.Previous);
        Assert.Equal(Role.Reader.Rights, set.Current);
        Assert.Equal(Admin, set.By);
        Assert.Equal(Any.Later(1), set.OccurredAt);
        Assert.Equal(list.Version, set.Version);
    }

    [Fact]
    public void No_access_takes_the_permission_away()
    {
        var list = EmptyList();
        list.SetRole(Any.Human, Role.Reader, Admin, Any.At);

        list.SetRole(Any.Human, Role.NoAccess, Admin, Any.Later(1));

        Assert.Empty(list.Permissions);
        Assert.Equal(NodeRight.None, list.RightsOf(Any.Human));
    }

    [Fact]
    public void No_access_for_an_actor_without_a_permission_changes_nothing()
    {
        var list = EmptyList();
        list.SetRole(Any.Human, Role.Reader, Admin, Any.At);
        list.DequeueDomainEvents();
        var version = list.Version;

        list.SetRole(Any.Client, Role.NoAccess, Admin, Any.Later(1));

        Assert.Equal(version, list.Version);
        Assert.Empty(list.DequeueDomainEvents());
    }

    [Fact]
    public void Permissions_are_listed_by_actor()
    {
        var list = EmptyList();

        list.SetRole(ActorId.Parse("zoe"), Role.Reader, Admin, Any.At);
        list.SetRole(ActorId.Parse("adam"), Role.Reader, Admin, Any.At);

        Assert.Equal(["adam", "zoe"], list.Permissions.Select(permission => permission.Actor.Value));
    }

    [Fact]
    public void Refuses_a_timestamp_that_is_not_utc()
    {
        var localTime = new DateTimeOffset(2026, 9, 19, 10, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<ArgumentException>(() => EmptyList().SetRole(Any.Human, Role.Reader, Admin, localTime));
    }
}
