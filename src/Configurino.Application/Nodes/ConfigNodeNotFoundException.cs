using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;

namespace org.g14.Configurino.Application.Nodes;

/// <summary>
/// No config node exists at the requested location
/// </summary>
public sealed class ConfigNodeNotFoundException : DomainException
{
    public ConfigNodeNotFoundException(NodePath path)
        : base($"No config node exists at '{path}'.")
    {
        Path = path;
    }

    public NodePath Path { get; }
}
