using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>The same key was assigned more than once within a single change set.</summary>
public sealed class DuplicateAssignmentException : DomainException
{
    public DuplicateAssignmentException(EntryKey key)
        : base($"Key '{key}' was assigned more than once in the same change set.")
    {
        Key = key;
    }

    public EntryKey Key { get; }
}
