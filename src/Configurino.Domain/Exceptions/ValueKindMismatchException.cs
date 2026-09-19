using org.g14.Configurino.Domain.ConfigTree.Config;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>A value was written whose kind differs from the kind the key was registered with.</summary>
public sealed class ValueKindMismatchException : DomainException
{
    public ValueKindMismatchException(EntryKey key, ValueKind expected, ValueKind actual)
        : base($"Key '{key}' is registered as {expected} but was given a {actual} value.")
    {
        Key = key;
        Expected = expected;
        Actual = actual;
    }

    public EntryKey Key { get; }

    public ValueKind Expected { get; }

    public ValueKind Actual { get; }
}
