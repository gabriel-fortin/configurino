using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Access.GetAccessList;

public sealed class GetAccessListHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly INodeDirectory _directory;
    private readonly INodeAccessListRepository _accessLists;

    public GetAccessListHandler(
        NodeLocator locator,
        AccessGuard access,
        INodeDirectory directory,
        INodeAccessListRepository accessLists)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(accessLists);

        _locator = locator;
        _access = access;
        _directory = directory;
        _accessLists = accessLists;
    }

    public async Task<AccessListView> Handle(GetAccessListQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        NodeId nodeId = await _locator.ResolveId(query.Path, cancellationToken);
        await _access.Demand(query.By, nodeId, query.Path, NodeRight.ManagePermissions, cancellationToken);

        IReadOnlyList<NodeId> ancestry = await _directory.GetAncestry(nodeId, cancellationToken);
        Dictionary<NodeId, NodeAccessList> lists = (await _accessLists.FindMany(ancestry, cancellationToken))
            .ToDictionary(list => list.NodeId);

        List<InheritedPermission> inherited = new List<InheritedPermission>();

        foreach (var ancestorId in ancestry.Where(id => id != nodeId))
        {
            if (!lists.TryGetValue(ancestorId, out var list) || list.Permissions.Count == 0)
            {
                continue;
            }

            // Nodes are never deleted, so an ancestor of a node that was just resolved always has a path.
            NodePath from = await _directory.GetPath(ancestorId, cancellationToken) ?? NodePath.Root;

            inherited.AddRange(list.Permissions.Select(permission => new InheritedPermission(from, permission)));
        }

        NodeAccessList? own = lists.GetValueOrDefault(nodeId);

        return new AccessListView(own?.Version ?? 0, own?.Permissions ?? [], inherited);
    }
}
