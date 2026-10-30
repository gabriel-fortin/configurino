using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Tree.CreateGroupingNode;

/// <summary>Creates a grouping node called <see cref="Name"/> under the grouping node at <see cref="Parent"/>.</summary>
public sealed record CreateGroupingNodeCommand(NodePath Parent, NodeName Name, ActorId By);
