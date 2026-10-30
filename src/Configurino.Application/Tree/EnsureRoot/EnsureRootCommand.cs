using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Application.Tree.EnsureRoot;

/// <summary>Makes sure the root grouping node exists. Meant to run at startup, any number of times.</summary>
public sealed record EnsureRootCommand(ActorId By);
