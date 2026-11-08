using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.GetChildren;

public sealed class GetChildrenHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly INodeDirectory _directory;

    public GetChildrenHandler(NodeLocator locator, AccessGuard access, INodeDirectory directory)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(directory);

        _locator = locator;
        _access = access;
        _directory = directory;
    }

    public async Task<IReadOnlyList<NodeSummary>> Handle(
        GetChildrenQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        NodeId id = await _locator.ResolveGroupingId(query.Path, cancellationToken);
        await _access.Demand(query.By, id, query.Path, NodeRight.Browse, cancellationToken);

        return await _directory.GetChildren(id, cancellationToken);
    }
}
