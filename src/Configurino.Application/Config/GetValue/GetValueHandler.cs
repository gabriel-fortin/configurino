using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Application.Config.GetValue;

public sealed class GetValueHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;

    public GetValueHandler(NodeLocator locator, AccessGuard access)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);

        _locator = locator;
        _access = access;
    }

    public async Task<KeyReadResult> Handle(GetValueQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var node = await _locator.LoadConfigNode(query.Path, cancellationToken);
        await _access.Demand(query.By, node.Id, query.Path, NodeRight.ReadConfig, cancellationToken);

        return node.TryGetValue(query.Key);
    }
}
