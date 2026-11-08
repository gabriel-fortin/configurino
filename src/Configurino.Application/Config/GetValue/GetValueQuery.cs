using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.GetValue;

/// <summary>Reads one key of the config node at <see cref="Path"/>.</summary>
public sealed record GetValueQuery(NodePath Path, EntryKey Key, ActorId By);
