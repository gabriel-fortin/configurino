using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access.GetEffectiveRights;

/// <summary>
/// Reads everything <see cref="By"/> may do on the node at <see cref="Path"/> — what a screen needs to
/// offer only the actions that will be allowed.
/// </summary>
public sealed record GetEffectiveRightsQuery(NodePath Path, ActorId By);
