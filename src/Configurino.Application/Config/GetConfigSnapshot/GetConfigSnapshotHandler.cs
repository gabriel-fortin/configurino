using org.g14.Configurino.Application.Nodes;

namespace org.g14.Configurino.Application.Config.GetConfigSnapshot;

public sealed class GetConfigSnapshotHandler
{
    private readonly NodeLocator _locator;

    public GetConfigSnapshotHandler(NodeLocator locator)
    {
        ArgumentNullException.ThrowIfNull(locator);

        _locator = locator;
    }

    public async Task<SnapshotResult> Handle(
        GetConfigSnapshotQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var node = await _locator.LoadConfigNode(query.Path, cancellationToken);

        return query.KnownVersion == node.Version
            ? new SnapshotResult.NotModified(node.Version)
            : new SnapshotResult.Changed(node.Snapshot());
    }
}
