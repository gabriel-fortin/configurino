using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.GetChildren;

public sealed class GetChildrenHandler
{
    private readonly NodeLocator _locator;
    private readonly INodeDirectory _directory;

    public GetChildrenHandler(NodeLocator locator, INodeDirectory directory)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(directory);

        _locator = locator;
        _directory = directory;
    }

    public async Task<IReadOnlyList<NodeSummary>> Handle(
        GetChildrenQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        NodeId id = await _locator.ResolveGroupingId(query.Path, cancellationToken);

        return await _directory.GetChildren(id, cancellationToken);
    }
}
