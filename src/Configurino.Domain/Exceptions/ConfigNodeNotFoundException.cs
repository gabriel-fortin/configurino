using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// No config node exists at the requested location. Config nodes are created by people, never
/// implicitly by a registering client, so a client's first deployment fails until someone creates it.
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
