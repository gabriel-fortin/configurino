using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Nodes;

/// <summary>A node exists at the requested path, but it is not the kind the operation needs.</summary>
public sealed class NodeKindMismatchException : UseCaseException
{
    public NodeKindMismatchException(NodePath path, NodeKind expected, NodeKind actual)
        : base($"The node at '{path}' is a {actual} node, but a {expected} node is required.")
    {
        Path = path;
        Expected = expected;
        Actual = actual;
    }

    public NodePath Path { get; }

    public NodeKind Expected { get; }

    public NodeKind Actual { get; }
}
