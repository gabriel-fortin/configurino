using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// An operation named a key that was never registered. Keys come into existence only through a client
/// application's registration, never through a human setting a value.
/// </summary>
public sealed class UnknownConfigKeyException : DomainException
{
    public UnknownConfigKeyException(ConfigKeyName key)
        : base($"Key '{key}' is not registered on this config node.")
    {
        Key = key;
    }

    public ConfigKeyName Key { get; }
}
