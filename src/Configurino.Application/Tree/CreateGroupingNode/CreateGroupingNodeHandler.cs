using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.CreateGroupingNode;

public sealed class CreateGroupingNodeHandler
{
    private readonly NodeLocator _locator;
    private readonly IGroupingNodeRepository _groupingNodes;
    private readonly TimeProvider _time;

    public CreateGroupingNodeHandler(
        NodeLocator locator,
        IGroupingNodeRepository groupingNodes,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(groupingNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _groupingNodes = groupingNodes;
        _time = time;
    }

    public async Task<CreatedNode> Handle(
        CreateGroupingNodeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        GroupingNode parent = await _locator.LoadGroupingNode(command.Parent, cancellationToken);

        GroupingNode child = parent.CreateChildGroup(command.Name, command.By, _time.GetUtcNow());

        await _groupingNodes.Add(child, cancellationToken);

        NodePath childPath = command.Parent.Append(command.Name);
        return new CreatedNode(child.Id, childPath);
    }
}
