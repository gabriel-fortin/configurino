using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access.GetAccessList;

/// <summary>Reads who may do what on the node at <see cref="Path"/>, and where each permission comes from.</summary>
public sealed record GetAccessListQuery(NodePath Path, ActorId By);
