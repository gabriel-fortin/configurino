using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.CreateConfigNode;

public sealed class CreateConfigNodeHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public CreateConfigNodeHandler(
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

    public async Task<CreatedNode> Handle(
        CreateConfigNodeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        GroupingNode parent = await _locator.LoadGroupingNode(command.Parent, cancellationToken);
        await _access.Demand(command.By, parent.Id, command.Parent, NodeRight.CreateNodes, cancellationToken);

        ConfigNode child = parent.CreateChildConfig(command.Name, command.By, _time.GetUtcNow());

        await _configNodes.Add(child, cancellationToken);

        NodePath childPath = command.Parent.Append(command.Name);
        return new CreatedNode(child.Id, childPath);
    }
}
