using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Application.Config.GetConfigSnapshot;

public sealed class GetConfigSnapshotHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;

    public GetConfigSnapshotHandler(NodeLocator locator, AccessGuard access)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);

        _locator = locator;
        _access = access;
    }

    public async Task<SnapshotResult> Handle(
        GetConfigSnapshotQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var node = await _locator.LoadConfigNode(query.Path, cancellationToken);
        await _access.Demand(query.By, node.Id, query.Path, NodeRight.ReadConfig, cancellationToken);

        return query.KnownVersion == node.Version
            ? new SnapshotResult.NotModified(node.Version)
            : new SnapshotResult.Changed(node.Snapshot());
    }
}
