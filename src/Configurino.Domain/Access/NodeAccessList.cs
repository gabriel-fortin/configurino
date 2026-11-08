using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Access;

/// <summary>
/// The permissions given on one node: at most one <see cref="Permission"/> per actor.
/// </summary>
/// <remarks>
/// <para>
/// An aggregate of its own rather than part of the node, so changing who may do what never moves a
/// config node's version on: a client polling for changes does not see one, and a person's edit does not
/// collide with an administrator's change of roles.
/// </para>
/// <para>
/// A node nobody has set a role on has no list stored at all, which means the same as an empty one.
/// What an actor may do on a node is the sum over this list and the lists of every node above it; see
/// <see cref="AccessEvaluator"/>.
/// </para>
/// </remarks>
public sealed class NodeAccessList : AggregateRoot
{
    private readonly Dictionary<ActorId, NodeRight> _permissions = [];

    private NodeAccessList(NodeId nodeId)
        : base(version: 0)
    {
        NodeId = nodeId;
    }

    public NodeId NodeId { get; }

    /// <summary>Ordered by actor, so the list reads the same however it was built up.</summary>
    public IReadOnlyList<Permission> Permissions =>
    [
        .. _permissions
            .OrderBy(permission => permission.Key.Value, StringComparer.Ordinal)
            .Select(permission => new Permission(permission.Key, permission.Value)),
    ];

    /// <summary>Starts the list of a node nobody has set a role on yet.</summary>
    public static NodeAccessList For(NodeId nodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeId);

        return new NodeAccessList(nodeId);
    }

    /// <summary>The rights given to an actor on this node itself, leaving out what it inherits.</summary>
    public NodeRight RightsOf(ActorId actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return _permissions.GetValueOrDefault(actor);
    }

    /// <summary>
    /// Makes what an actor holds on this node exactly the rights of <paramref name="role"/>, whatever it
    /// held here before; <see cref="Role.NoAccess"/> takes its permission away. Rights it inherits from
    /// above are untouched — they can only be changed where they were given. Setting what the actor
    /// already holds changes nothing.
    /// </summary>
    public void SetRole(ActorId actor, Role role, ActorId by, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(role);
        ArgumentNullException.ThrowIfNull(by);
        at = RequireUtc(at, nameof(at));

        NodeRight previous = RightsOf(actor);
        NodeRight newRights = role.Rights;

        if (newRights == previous)
        {
            return;
        }

        if (newRights == NodeRight.None)
        {
            _permissions.Remove(actor);
        }
        else
        {
            _permissions[actor] = newRights;
        }

        MarkChanged();
        Raise(new RoleSet(NodeId, actor, previous, newRights, by, at, Version));
    }

    public override string ToString() => $"access list of {NodeId}";
}
