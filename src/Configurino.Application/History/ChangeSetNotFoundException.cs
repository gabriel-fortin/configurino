using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.History;

/// <summary>No such change set exists on the config node at the given path.</summary>
public sealed class ChangeSetNotFoundException : UseCaseException
{
    public ChangeSetNotFoundException(ChangeSetId changeSetId, NodePath path)
        : base($"No change set {changeSetId} exists on the config node at '{path}'.")
    {
        ChangeSetId = changeSetId;
        Path = path;
    }

    public ChangeSetId ChangeSetId { get; }

    public NodePath Path { get; }
}
