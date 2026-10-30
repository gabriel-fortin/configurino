using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.History.GetChangeSet;

public sealed class GetChangeSetHandler
{
    private readonly NodeLocator _locator;
    private readonly IConfigChangeLog _changeLog;

    public GetChangeSetHandler(NodeLocator locator, IConfigChangeLog changeLog)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(changeLog);

        _locator = locator;
        _changeLog = changeLog;
    }

    public async Task<ConfigChangeEntry> Handle(GetChangeSetQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var nodeId = await _locator.ResolveConfigId(query.Path, cancellationToken);
        var entry = await _changeLog.GetChangeSet(query.ChangeSetId, cancellationToken);

        // A change set of some other node is, from this path's point of view, simply not there.
        return entry is not null && entry.NodeId == nodeId
            ? entry
            : throw new ChangeSetNotFoundException(query.ChangeSetId, query.Path);
    }
}
