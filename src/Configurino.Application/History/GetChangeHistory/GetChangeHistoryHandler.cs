using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.History.GetChangeHistory;

public sealed class GetChangeHistoryHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly IConfigChangeLog _changeLog;

    public GetChangeHistoryHandler(NodeLocator locator, AccessGuard access, IConfigChangeLog changeLog)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(changeLog);

        _locator = locator;
        _access = access;
        _changeLog = changeLog;
    }

    public async Task<IReadOnlyList<ConfigChangeEntry>> Handle(
        GetChangeHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var nodeId = await _locator.ResolveConfigId(query.Path, cancellationToken);
        await _access.Demand(query.By, nodeId, query.Path, NodeRight.ReadHistory, cancellationToken);

        return await _changeLog.GetChanges(nodeId, query.Limit, query.Before, cancellationToken);
    }
}
