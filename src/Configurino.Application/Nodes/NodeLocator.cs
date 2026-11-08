using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Nodes;

/// <summary>
/// Turns a path into a node represented by that path (or just its id)
/// Says precisely when this is not possible:
/// nothing there, or the wrong kind of thing there.
/// </summary>
public sealed class NodeLocator
{
    private readonly INodeDirectory _directory;
    private readonly IGroupingNodeRepository _groupingNodes;
    private readonly IConfigNodeRepository _configNodes;

    public NodeLocator(
        INodeDirectory directory,
        IGroupingNodeRepository groupingNodes,
        IConfigNodeRepository configNodes)
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(groupingNodes);
        ArgumentNullException.ThrowIfNull(configNodes);

        _directory = directory;
        _groupingNodes = groupingNodes;
        _configNodes = configNodes;
    }

    /// <summary>Finds the id of the node at a path, whatever its kind, without loading it.</summary>
    /// <exception cref="NodeNotFoundException">
    /// Nothing exists at <paramref name="path"/>.
    /// </exception>
    public async Task<NodeId> ResolveId(NodePath path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        return await _directory.Resolve(path, cancellationToken) ?? throw new NodeNotFoundException(path);
    }

    /// <summary>Finds the id of the grouping node at a path, without loading it.</summary>
    /// <exception cref="GroupingNodeNotFoundException">
    /// Nothing exists at <paramref name="path"/>.
    /// </exception>
    /// <exception cref="NodeKindMismatchException">
    /// The node at <paramref name="path"/> is not a grouping node.
    /// </exception>
    public async Task<NodeId> ResolveGroupingId(NodePath path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        NodeId id = await _directory.Resolve(path, cancellationToken)
                    ?? throw new GroupingNodeNotFoundException(path);

        await RequireKind(id, path, expected: NodeKind.Grouping, cancellationToken);

        return id;
    }

    /// <summary>Finds the id of the config node at a path, without loading it.</summary>
    /// <exception cref="ConfigNodeNotFoundException">
    /// Nothing exists at <paramref name="path"/>.
    /// </exception>
    /// <exception cref="NodeKindMismatchException">
    /// The node at <paramref name="path"/> is a grouping node.
    /// </exception>
    public async Task<NodeId> ResolveConfigId(NodePath path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(path);

        NodeId id = await _directory.Resolve(path, cancellationToken)
                    ?? throw new ConfigNodeNotFoundException(path);

        await RequireKind(id, path, expected: NodeKind.Config, cancellationToken);

        return id;
    }

    /// <summary>Loads the grouping node at a path, so the caller can create something under it.</summary>
    /// <exception cref="GroupingNodeNotFoundException">
    /// Nothing exists at <paramref name="path"/>, or the node vanished between being resolved and being loaded.
    /// </exception>
    /// <exception cref="NodeKindMismatchException">
    /// The node at <paramref name="path"/> is a config node.
    /// </exception>
    public async Task<GroupingNode> LoadGroupingNode(NodePath path, CancellationToken cancellationToken = default)
    {
        NodeId id = await ResolveGroupingId(path, cancellationToken);

        return await _groupingNodes.Find(id, cancellationToken) ?? throw new GroupingNodeNotFoundException(path);
    }

    /// <summary>Loads the config node at a path, ready to be read or changed.</summary>
    /// <exception cref="ConfigNodeNotFoundException">
    /// Nothing exists at <paramref name="path"/>, or the node vanished between being resolved and being loaded.
    /// </exception>
    /// <exception cref="NodeKindMismatchException">
    /// The node at <paramref name="path"/> is a grouping node.
    /// </exception>
    public async Task<ConfigNode> LoadConfigNode(NodePath path, CancellationToken cancellationToken = default)
    {
        NodeId id = await ResolveConfigId(path, cancellationToken);

        return await _configNodes.Find(id, cancellationToken) ?? throw new ConfigNodeNotFoundException(path);
    }

    private async Task RequireKind(NodeId id, NodePath path, NodeKind expected, CancellationToken cancellationToken)
    {
        NodeKind? actual = await _directory.GetKind(id, cancellationToken);

        if (actual is not null && actual != expected)
        {
            throw new NodeKindMismatchException(path, expected, actual.Value);
        }
    }
}
