using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Tests;

public sealed class AccessEvaluatorTests
{
    private static readonly ActorId Admin = ActorId.Parse("admin");

    private static NodeAccessList ListGiving(ActorId actor, Role role)
    {
        var list = NodeAccessList.For(NodeId.New());
        list.SetRole(actor, role, Admin, Any.At);

        return list;
    }

    [Fact]
    public void Without_any_list_nobody_may_do_anything()
    {
        Assert.Equal(NodeRight.None, AccessEvaluator.EffectiveRights(Any.Human, []));
    }

    [Fact]
    public void Rights_given_anywhere_along_the_way_add_up()
    {
        NodeAccessList[] lineage =
        [
            ListGiving(Any.Human, Role.Reader),
            ListGiving(Any.Human, Role.ClientApp),
        ];

        Assert.Equal(
            Role.Reader.Rights | Role.ClientApp.Rights,
            AccessEvaluator.EffectiveRights(Any.Human, lineage));
    }

    [Fact]
    public void A_right_given_high_up_cannot_be_narrowed_further_down()
    {
        NodeAccessList[] lineage = [ListGiving(Any.Human, Role.Editor), ListGiving(Any.Human, Role.NoAccess)];

        Assert.Equal(Role.Editor.Rights, AccessEvaluator.EffectiveRights(Any.Human, lineage));
    }

    [Fact]
    public void Rights_of_other_actors_do_not_count()
    {
        NodeAccessList[] lineage = [ListGiving(Any.Client, Role.Owner)];

        Assert.Equal(NodeRight.None, AccessEvaluator.EffectiveRights(Any.Human, lineage));
    }
}
