using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access.SetRole;

/// <summary>
/// Makes what <see cref="Subject"/> holds on the node at <see cref="Path"/> — and so on everything below it —
/// exactly the rights of <see cref="Role"/>. <see cref="Domain.Access.Role.NoAccess"/> takes its
/// permission there away; rights it inherits from above stay.
/// </summary>
public sealed record SetRoleCommand(NodePath Path, ActorId Subject, Role Role, ActorId By);
