using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Application.Config.GetValue;

public sealed class GetValueHandler
{
    private readonly NodeLocator _locator;

    public GetValueHandler(NodeLocator locator)
    {
        ArgumentNullException.ThrowIfNull(locator);

        _locator = locator;
    }

    public async Task<KeyReadResult> Handle(GetValueQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var node = await _locator.LoadConfigNode(query.Path, cancellationToken);

        return node.TryGetValue(query.Key);
    }
}
