using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Access;

/// <summary>The node exists, but the actor does not hold the right the operation needs on it.</summary>
public sealed class AccessDeniedException : UseCaseException
{
    public AccessDeniedException(ActorId actor, NodePath path, NodeRight required)
        : base($"'{actor}' does not hold the {required} right on '{path}'.")
    {
        Actor = actor;
        Path = path;
        Required = required;
    }

    public ActorId Actor { get; }

    public NodePath Path { get; }

    public NodeRight Required { get; }
}
