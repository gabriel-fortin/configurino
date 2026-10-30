using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.Rollback;

/// <summary>A person puts back what the change set <see cref="Target"/> changed on the config node at <see cref="Path"/>.</summary>
public sealed record RollbackCommand(NodePath Path, ChangeSetId Target, ActorId By);
