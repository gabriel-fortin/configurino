using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Nodes;

/// <summary>No node of any kind exists at the requested path.</summary>
public sealed class NodeNotFoundException : UseCaseException
{
    public NodeNotFoundException(NodePath path)
        : base($"No node exists at '{path}'.")
    {
        Path = path;
    }

    public NodePath Path { get; }
}
