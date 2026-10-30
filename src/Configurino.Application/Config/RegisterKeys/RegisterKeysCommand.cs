using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.RegisterKeys;

/// <summary>A client application declares the keys it uses on the config node at <see cref="Path"/>.</summary>
public sealed record RegisterKeysCommand(NodePath Path, IReadOnlyCollection<KeyRegistration> Keys, ActorId By);
