using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tree.EnsureRoot;

public sealed class EnsureRootHandler
{
    private readonly IGroupingNodeRepository _groupingNodes;
    private readonly TimeProvider _time;

    public EnsureRootHandler(IGroupingNodeRepository groupingNodes, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(groupingNodes);
        ArgumentNullException.ThrowIfNull(time);

        _groupingNodes = groupingNodes;
        _time = time;
    }

    public async Task Handle(EnsureRootCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await _groupingNodes.Find(NodeId.Root, cancellationToken) is not null)
        {
            return;
        }

        var root = GroupingNode.CreateRoot(command.By, _time.GetUtcNow());

        try
        {
            await _groupingNodes.Add(root, cancellationToken);
        }
        catch (Exception)
        {
            // Another instance may have created the root between our look and our write. The root has a
            // fixed id, so that is the outcome we wanted: only a root still missing is a real failure.
            if (await _groupingNodes.Find(NodeId.Root, cancellationToken) is null)
            {
                throw;
            }
        }
    }
}
