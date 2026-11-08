using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.ConfigTree;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Events;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Tests;

/// <summary>
/// One in-memory stand-in for all the domain ports, so a test can use the handlers end to end.
/// It keeps live aggregate instances, so it does not model version-based conflicts; a test that needs
/// them supplies its own repository.
/// </summary>
public sealed class InMemoryStore :
    IGroupingNodeRepository,
    IConfigNodeRepository,
    INodeAccessListRepository,
    INodeDirectory,
    IConfigChangeLog
{
    private readonly Dictionary<NodeId, GroupingNode> _groupingNodes = [];
    private readonly Dictionary<NodeId, ConfigNode> _configNodes = [];
    private readonly Dictionary<NodeId, NodeAccessList> _accessLists = [];
    private readonly List<ConfigChangeEntry> _changes = [];
    private readonly List<IDomainEvent> _accessChanges = [];

    public int SaveCalls { get; private set; }

    public IReadOnlyList<ConfigChangeEntry> AllChanges => _changes;

    /// <summary>The grant and revoke events of every access list stored, in the order they were stored.</summary>
    public IReadOnlyList<IDomainEvent> AllAccessChanges => _accessChanges;

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

    Task<NodeAccessList?> INodeAccessListRepository.Find(NodeId nodeId, CancellationToken cancellationToken) =>
        Task.FromResult(_accessLists.GetValueOrDefault(nodeId));

    public Task<IReadOnlyList<NodeAccessList>> FindMany(
        IReadOnlyCollection<NodeId> nodeIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<NodeAccessList>>(
            [.. nodeIds.Where(_accessLists.ContainsKey).Select(id => _accessLists[id])]);

    public Task Add(NodeAccessList list, CancellationToken cancellationToken = default)
    {
        if (_accessLists.TryGetValue(list.NodeId, out var existing))
        {
            throw new ConcurrencyConflictException(list.NodeId, expectedVersion: 0, existing.Version);
        }

        _accessLists.Add(list.NodeId, list);
        _accessChanges.AddRange(list.DequeueDomainEvents());

        return Task.CompletedTask;
    }

    public Task Save(NodeAccessList list, CancellationToken cancellationToken = default)
    {
        _accessChanges.AddRange(list.DequeueDomainEvents());

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

    public Task<IReadOnlyList<NodeId>> GetAncestry(NodeId id, CancellationToken cancellationToken = default)
    {
        var ancestry = new Stack<NodeId>();
        NodeId? current = id;

        while (current is not null)
        {
            if (_groupingNodes.TryGetValue(current, out var group))
            {
                ancestry.Push(current);
                current = group.ParentId;
            }
            else if (_configNodes.TryGetValue(current, out var config))
            {
                ancestry.Push(current);
                current = config.ParentId;
            }
            else
            {
                return Task.FromResult<IReadOnlyList<NodeId>>([]);
            }
        }

        return Task.FromResult<IReadOnlyList<NodeId>>([.. ancestry]);
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
