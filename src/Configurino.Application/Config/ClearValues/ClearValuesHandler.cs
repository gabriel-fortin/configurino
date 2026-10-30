using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Config.ClearValues;

public sealed class ClearValuesHandler
{
    private readonly NodeLocator _locator;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public ClearValuesHandler(NodeLocator locator, IConfigNodeRepository configNodes, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(configNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _configNodes = configNodes;
        _time = time;
    }

    /// <returns>The version the node is at once the values have been cleared.</returns>
    public async Task<int> Handle(ClearValuesCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var node = await _locator.LoadConfigNode(command.Path, cancellationToken);
        var versionBefore = node.Version;

        node.ClearValues(command.Keys, command.By, _time.GetUtcNow(), ChangeReason.Manual(command.Note));

        if (node.Version != versionBefore)
        {
            await _configNodes.Save(node, cancellationToken);
        }

        return node.Version;
    }
}
