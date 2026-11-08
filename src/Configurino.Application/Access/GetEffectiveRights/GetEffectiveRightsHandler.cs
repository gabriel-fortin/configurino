using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access.GetEffectiveRights;

public sealed class GetEffectiveRightsHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;

    public GetEffectiveRightsHandler(NodeLocator locator, AccessGuard access)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);

        _locator = locator;
        _access = access;
    }

    public async Task<NodeRight> Handle(GetEffectiveRightsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        NodeId nodeId = await _locator.ResolveId(query.Path, cancellationToken);
        await _access.Demand(query.By, nodeId, query.Path, NodeRight.Browse, cancellationToken);

        return await _access.EffectiveRights(query.By, nodeId, cancellationToken);
    }
}
