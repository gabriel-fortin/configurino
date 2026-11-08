using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Config.SetValues;

public sealed class SetValuesHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public SetValuesHandler(
        NodeLocator locator,
        AccessGuard access,
        IConfigNodeRepository configNodes,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(configNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _access = access;
        _configNodes = configNodes;
        _time = time;
    }

    /// <remarks>
    /// Deliberately not retried on a conflict: it means the person edited a node that has since moved on,
    /// and silently re-applying their edit over someone else's is not for the handler to decide.
    /// </remarks>
    /// <returns>The version the node is at once the values have been set.</returns>
    public async Task<int> Handle(SetValuesCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var node = await _locator.LoadConfigNode(command.Path, cancellationToken);
        await _access.Demand(command.By, node.Id, command.Path, NodeRight.EditValues, cancellationToken);

        var versionBefore = node.Version;

        node.SetValues(command.Assignments, command.By, _time.GetUtcNow(), ChangeReason.Manual(command.Note));

        if (node.Version != versionBefore)
        {
            await _configNodes.Save(node, cancellationToken);
        }

        return node.Version;
    }
}
