using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.History;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Config.Rollback;

public sealed class RollbackHandler
{
    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly IConfigChangeLog _changeLog;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public RollbackHandler(
        NodeLocator locator,
        AccessGuard access,
        IConfigChangeLog changeLog,
        IConfigNodeRepository configNodes,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(changeLog);
        ArgumentNullException.ThrowIfNull(configNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _access = access;
        _changeLog = changeLog;
        _configNodes = configNodes;
        _time = time;
    }

    /// <remarks>
    /// Needs no unit of work: it writes one aggregate. The change set read from the log is only an input
    /// that is compared with the loaded node, and saving fails if the node moved on since it was loaded.
    /// </remarks>
    /// <returns>The version the node is at once the change set has been undone.</returns>
    public async Task<int> Handle(RollbackCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var node = await _locator.LoadConfigNode(command.Path, cancellationToken);
        await _access.Demand(command.By, node.Id, command.Path, NodeRight.EditValues, cancellationToken);

        var entry = await _changeLog.GetChangeSet(command.Target, cancellationToken);

        if (entry is null || entry.NodeId != node.Id)
        {
            throw new ChangeSetNotFoundException(command.Target, command.Path);
        }

        if (entry.SchemaChanges.Count > 0)
        {
            throw new ChangeSetNotRollbackableException(command.Target);
        }

        var conflicts = entry.ValueChanges
            .Where(change => CurrentValueOf(node.TryGetValue(change.Key)) != change.Current)
            .Select(change => change.Key)
            .ToArray();

        if (conflicts.Length > 0)
        {
            throw new RollbackConflictException(command.Target, conflicts);
        }

        var versionBefore = node.Version;
        var reason = ChangeReason.Rollback(command.Target);
        var now = _time.GetUtcNow();

        var toRestore = entry.ValueChanges
            .Where(change => change.Previous is not null)
            .Select(change => new KeyAssignment(change.Key, change.Previous!))
            .ToArray();

        var toClear = entry.ValueChanges
            .Where(change => change.Previous is null)
            .Select(change => change.Key)
            .ToArray();

        if (toRestore.Length > 0)
        {
            node.SetValues(toRestore, command.By, now, reason);
        }

        if (toClear.Length > 0)
        {
            node.ClearValues(toClear, command.By, now, reason);
        }

        if (node.Version != versionBefore)
        {
            await _configNodes.Save(node, cancellationToken);
        }

        return node.Version;
    }

    private static EntryValue? CurrentValueOf(KeyReadResult result) =>
        result is KeyReadResult.SetKey set ? set.Value : null;
}
