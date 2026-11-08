                 using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.EnsureRoot;

public sealed class EnsureRootHandler
{
    private readonly IGroupingNodeRepository _groupingNodes;
    private readonly INodeAccessListRepository _accessLists;
    private readonly TimeProvider _time;

    public EnsureRootHandler(
        IGroupingNodeRepository groupingNodes,
        INodeAccessListRepository accessLists,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(groupingNodes);
        ArgumentNullException.ThrowIfNull(accessLists);
        ArgumentNullException.ThrowIfNull(time);

        _groupingNodes = groupingNodes;
        _accessLists = accessLists;
        _time = time;
    }

    /// <remarks>
    /// <para>
    /// Deliberately checks no permission: before the root exists nobody holds any, so this is the one way
    /// in. Whoever comes first becomes the owner; from then on it gives nobody anything.
    /// </para>
    /// <para>
    /// The root and its access list are ensured one after the other, each on its own: a run that died
    /// between the two leaves a root nobody can administer, and the next caller repairs it — becoming its
    /// owner, just as the first caller would have.
    /// </para>
    /// </remarks>
    public async Task Handle(EnsureRootCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await EnsureRootNode(command, cancellationToken);
        await EnsureRootAccessList(command, cancellationToken);
    }

    private async Task EnsureRootNode(EnsureRootCommand command, CancellationToken cancellationToken)
    {
        if (await _groupingNodes.Find(NodeId.Root, cancellationToken) is not null)
        {
            return;
        }

        GroupingNode root = GroupingNode.CreateRoot(command.By, _time.GetUtcNow());

        try
        {
            await _groupingNodes.Add(root, cancellationToken);
        }
        catch (Exception)
        {
            // Another instance may have created the root between our look and our write. The root has a
            // fixed id, so that is the outcome we wanted: only a root still missing is a real failure.
            if (await _groupingNodes.Find(NodeId.Root, cancellationToken) is null)
            {
                throw;
            }
        }
    }

    private async Task EnsureRootAccessList(EnsureRootCommand command, CancellationToken cancellationToken)
    {
        if (await _accessLists.Find(NodeId.Root, cancellationToken) is not null)
        {
            return;
        }

        NodeAccessList list = NodeAccessList.For(NodeId.Root);
        list.SetRole(command.By, Role.Owner, command.By, _time.GetUtcNow());

        try
        {
            await _accessLists.Add(list, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // Another instance gave the root its list first; whoever it made owner stays owner.
        }
    }
}
