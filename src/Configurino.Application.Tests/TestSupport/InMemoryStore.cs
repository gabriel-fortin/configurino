using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tests;

/// <summary>
/// One in-memory stand-in for all four domain ports, so a test can use the handlers end to end.
/// It keeps live aggregate instances, so it does not model version-based conflicts; a test that needs
/// them supplies its own repository.
/// </summary>
public sealed class InMemoryStore :
    IGroupingNodeRepository,
    IConfigNodeRepository,
    INodeDirectory,
    IConfigChangeLog
{
    private readonly Dictionary<NodeId, GroupingNode> _groupingNodes = [];
    private readonly Dictionary<NodeId, ConfigNode> _configNodes = [];
    private readonly List<ConfigChangeEntry> _changes = [];

    public int SaveCalls { get; private set; }

    public IReadOnlyList<ConfigChangeEntry> AllChanges => _changes;

    // --- repositories ---------------------------------------------------------------------------

    Task<GroupingNode?> IGroupingNodeRepository.Find(NodeId id, CancellationToken cancellationToken) =>
        Task.FromResult(_groupingNodes.GetValueOrDefault(id));

    Task<ConfigNode?> IConfigNodeRepository.Find(NodeId id, CancellationToken cancellationToken) =>
        Task.FromResult(_configNodes.GetValueOrDefault(id));

    public Task Add(GroupingNode node, CancellationToken cancellationToken = default)
    {
        RequireFreeName(node.ParentId, node.Name);
        node.DequeueDomainEvents();
        _groupingNodes.Add(node.Id, node);

        return Task.CompletedTask;
    }

    public Task Add(ConfigNode node, CancellationToken cancellationToken = default)
    {
        RequireFreeName(node.ParentId, node.Name);
        node.DequeueDomainEvents();
        _configNodes.Add(node.Id, node);

        return Task.CompletedTask;
    }

    public Task Save(ConfigNode node, CancellationToken cancellationToken = default)
    {
        SaveCalls++;

        foreach (var domainEvent in node.DequeueDomainEvents())
        {
            switch (domainEvent)
            {
                case ConfigValuesChanged changed:
                    _changes.Add(ConfigChangeEntry.From(changed));
                    break;

                case ConfigKeysRegistered registered:
                    _changes.Add(ConfigChangeEntry.From(registered));
                    break;
            }
        }

        return Task.CompletedTask;
    }

    // --- directory ------------------------------------------------------------------------------

    public Task<NodeId?> Resolve(NodePath path, CancellationToken cancellationToken = default)
    {
        NodeId? current = _groupingNodes.ContainsKey(NodeId.Root) ? NodeId.Root : null;

        foreach (var segment in path.Segments)
        {
            current = current is null
                ? null
                : Children(current).FirstOrDefault(child => child.Name == segment)?.Id;
        }

        return Task.FromResult(current);
    }

    public Task<NodePath?> GetPath(NodeId id, CancellationToken cancellationToken = default)
    {
        var path = NodePath.Root;
        var segments = new Stack<NodeName>();
        NodeId? current = id;

        while (current is not null && !current.IsRoot)
        {
            if (_groupingNodes.TryGetValue(current, out var group))
            {
                segments.Push(group.Name!);
                current = group.ParentId;
            }
            else if (_configNodes.TryGetValue(current, out var config))
            {
                segments.Push(config.Name);
                current = config.ParentId;
            }
            else
            {
                return Task.FromResult<NodePath?>(null);
            }
        }

        foreach (var segment in segments)
        {
            path = path.Append(segment);
        }

        return Task.FromResult<NodePath?>(path);
    }

    public Task<NodeKind?> GetKind(NodeId id, CancellationToken cancellationToken = default) =>
        Task.FromResult<NodeKind?>(
            _groupingNodes.ContainsKey(id) ? NodeKind.Grouping
            : _configNodes.ContainsKey(id) ? NodeKind.Config
            : null);

    public Task<IReadOnlyList<NodeSummary>> GetChildren(NodeId id, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<NodeSummary>>(
            [.. Children(id).OrderBy(child => child.Name.Value, StringComparer.Ordinal)]);

    // --- change log -----------------------------------------------------------------------------

    public Task<IReadOnlyList<ConfigChangeEntry>> GetChanges(
        NodeId nodeId,
        int limit,
        ChangeSetId? before = null,
        CancellationToken cancellationToken = default)
    {
        var newestFirst = _changes
            .Where(entry => entry.NodeId == nodeId)
            .OrderByDescending(entry => entry.NodeVersion)
            .ToList();

        var start = before is null ? 0 : newestFirst.FindIndex(entry => entry.Id == before) + 1;

        return Task.FromResult<IReadOnlyList<ConfigChangeEntry>>([.. newestFirst.Skip(start).Take(limit)]);
    }

    public Task<ConfigChangeEntry?> GetChangeSet(ChangeSetId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_changes.FirstOrDefault(entry => entry.Id == id));

    // --- helpers --------------------------------------------------------------------------------

    private IEnumerable<NodeSummary> Children(NodeId parent) =>
        _groupingNodes.Values
            .Where(node => node.ParentId == parent)
            .Select(node => new NodeSummary(node.Id, node.Name!, NodeKind.Grouping))
            .Concat(
                _configNodes.Values
                    .Where(node => node.ParentId == parent)
                    .Select(node => new NodeSummary(node.Id, node.Name, NodeKind.Config)));

    private void RequireFreeName(NodeId? parentId, NodeName? name)
    {
        if (parentId is not null && name is not null && Children(parentId).Any(child => child.Name == name))
        {
            throw new NodeNameAlreadyUsedException(parentId, name);
        }
    }
}
