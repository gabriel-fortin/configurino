using org.g14.Configurino.Domain.ConfigTree.Changes;

namespace org.g14.Configurino.Application.Config.Rollback;

/// <summary>
/// The change set altered the key schema, which a client application's registration owns. Putting values
/// back is a person's decision; undoing a deployment's declaration is not.
/// </summary>
public sealed class ChangeSetNotRollbackableException : UseCaseException
{
    public ChangeSetNotRollbackableException(ChangeSetId changeSetId)
        : base($"Change set {changeSetId} changed the key schema and cannot be rolled back.")
    {
        ChangeSetId = changeSetId;
    }

    public ChangeSetId ChangeSetId { get; }
}
