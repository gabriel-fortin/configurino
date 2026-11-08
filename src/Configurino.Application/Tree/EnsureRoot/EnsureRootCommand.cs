using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Application.Tree.EnsureRoot;

/// <summary>
/// Makes sure the root grouping node exists and that someone can administer it. Needs no permission:
/// it is how the application gets into a usable state. Safe to run any number of times.
/// </summary>
/// <param name="By">
/// Who is asking. When the root has no access list yet — the very first use of the application —
/// this actor is made <see cref="Role.Owner"/> of the root, and so of the whole tree. Once the root has a
/// list, running this again gives nobody anything: permissions are managed from then on.
/// </param>
public sealed record EnsureRootCommand(ActorId By);
