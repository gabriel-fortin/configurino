using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// A value was written to a key no client application declares any more. The key and its value are
/// kept for compatibility, but changing it is meaningless until a registration reactivates it.
/// </summary>
public sealed class ObsoleteKeyException : DomainException
{
    public ObsoleteKeyException(ConfigKeyName key)
        : base($"Key '{key}' is obsolete; it must be registered again before its value can change.")
    {
        Key = key;
    }

    public ConfigKeyName Key { get; }
}
