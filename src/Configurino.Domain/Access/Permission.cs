namespace org.g14.Configurino.Domain.Access;

/// <summary>
/// Who may do what on a node: an actor and the rights it holds there.
/// </summary>
/// <remarks>
/// A <see cref="NodeRight"/> is something that can be done; a permission is an actor being allowed to do
/// a set of those things — the rights of the <see cref="Role"/> it was last set to. It belongs to the
/// <see cref="NodeAccessList"/> of one node and reaches every node below it.
/// </remarks>
public sealed record Permission
{
    public Permission(ActorId actor, NodeRight rights)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;
        Rights = rights;
    }

    public ActorId Actor { get; }

    public NodeRight Rights { get; }
}
