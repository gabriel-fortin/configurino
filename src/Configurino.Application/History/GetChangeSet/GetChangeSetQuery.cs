using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.History.GetChangeSet;

/// <summary>Reads one change set of the config node at <see cref="Path"/>.</summary>
public sealed record GetChangeSetQuery(NodePath Path, ChangeSetId ChangeSetId, ActorId By);
