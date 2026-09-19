using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>The same key was declared more than once within a single registration batch.</summary>
public sealed class DuplicateKeyDeclarationException : DomainException
{
    public DuplicateKeyDeclarationException(EntryKey key)
        : base($"Key '{key}' was declared more than once in the same registration.")
    {
        Key = key;
    }

    public EntryKey Key { get; }
}
