using org.g14.Configurino.Domain.ChangeLog;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Repositories;

/// <summary>
/// Reads a config node's history. Writing to it belongs to <see cref="IConfigNodeRepository.Save"/>,
/// which appends entries in the same transaction as the change they describe.
/// </summary>
public interface IConfigChangeLog
{
    /// <summary>
    /// The changes made to a node, newest first. <paramref name="before"/> continues from where a previous
    /// page ended, so a node with years of history can be read a screen at a time.
    /// </summary>
    Task<IReadOnlyList<ConfigChangeEntry>> GetChanges(
        NodeId nodeId,
        int limit,
        ChangeSetId? before = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads one change set — what a rollback is built from.</summary>
    Task<ConfigChangeEntry?> GetChangeSet(ChangeSetId id, CancellationToken cancellationToken = default);
}
