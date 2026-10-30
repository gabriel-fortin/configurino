using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.CreateConfigNode;

public sealed class CreateConfigNodeHandler
{
    private readonly NodeLocator _locator;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public CreateConfigNodeHandler(
        NodeLocator locator,
        IConfigNodeRepository configNodes,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(configNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _configNodes = configNodes;
        _time = time;
    }

    public async Task<CreatedNode> Handle(
        CreateConfigNodeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        GroupingNode parent = await _locator.LoadGroupingNode(command.Parent, cancellationToken);

        ConfigNode child = parent.CreateChildConfig(command.Name, command.By, _time.GetUtcNow());

        await _configNodes.Add(child, cancellationToken);

        NodePath childPath = command.Parent.Append(command.Name);
        return new CreatedNode(child.Id, childPath);
    }
}
