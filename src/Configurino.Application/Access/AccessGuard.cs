using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Access;

/// <summary>
/// Decides whether an actor may do something on a node, from the access lists of the node and of every
/// node above it.
/// </summary>
/// <remarks>
/// <para>
/// A helper for handlers, which decide for themselves which right their use case needs. A handler locates
/// the node first and asks here right after, before doing anything — so a missing node is still reported
/// as missing, whoever asks.
/// </para>
/// </remarks>
public sealed class AccessGuard
{
    private readonly INodeDirectory _directory;
    private readonly INodeAccessListRepository _accessLists;

    public AccessGuard(INodeDirectory directory, INodeAccessListRepository accessLists)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(accessLists);

        _directory = directory;
        _accessLists = accessLists;
    }

    /// <summary>Everything an actor may do on a node, whether given on the node itself or above it.</summary>
    public async Task<NodeRight> EffectiveRights(ActorId actor, NodeId nodeId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(nodeId);

        IReadOnlyList<NodeId> ancestry = await _directory.GetAncestry(nodeId, cancellationToken);
        IReadOnlyList<NodeAccessList> lists = ancestry.Count == 0
            ? []
            : await _accessLists.FindMany(ancestry, cancellationToken);

        return AccessEvaluator.EffectiveRights(actor, lists);
    }

    /// <exception cref="AccessDeniedException">
    /// The actor lacks any of the <paramref name="required"/> rights on the node.
    /// </exception>
    public async Task Demand(
        ActorId actor,
        NodeId nodeId,
        NodePath path,
        NodeRight required,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentOutOfRangeException.ThrowIfEqual(required, NodeRight.None);

        var held = await EffectiveRights(actor, nodeId, cancellationToken);

        if ((held & required) != required)
        {
            throw new AccessDeniedException(actor, path, required);
        }
    }
}
