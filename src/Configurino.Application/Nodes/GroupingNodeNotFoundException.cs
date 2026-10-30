using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Nodes;

/// <summary>No node exists at the requested path.</summary>
public sealed class GroupingNodeNotFoundException : UseCaseException
{
    public GroupingNodeNotFoundException(NodePath path)
        : base($"No grouping node exists at '{path}'.")
    {
        Path = path;
    }

    public NodePath Path { get; }
}
