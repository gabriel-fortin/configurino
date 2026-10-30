using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Application.Config.Rollback;

/// <summary>
/// Some keys have been changed again since the change set being undone, so putting the old values back
/// would silently overwrite a later edit.
/// </summary>
public sealed class RollbackConflictException : UseCaseException
{
    public RollbackConflictException(ChangeSetId changeSetId, IReadOnlyCollection<EntryKey> keys)
        : base($"Change set {changeSetId} cannot be rolled back: {string.Join(", ", keys)} changed since.")
    {
        ChangeSetId = changeSetId;
        Keys = keys;
    }

    public ChangeSetId ChangeSetId { get; }

    public IReadOnlyCollection<EntryKey> Keys { get; }
}
