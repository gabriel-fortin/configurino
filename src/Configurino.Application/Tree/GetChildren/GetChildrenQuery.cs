using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Tree.GetChildren;

/// <summary>Lists what sits directly under the grouping node at <see cref="Path"/>.</summary>
public sealed record GetChildrenQuery(NodePath Path, ActorId By);
