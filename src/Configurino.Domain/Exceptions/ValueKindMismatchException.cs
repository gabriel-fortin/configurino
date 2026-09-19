using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>A value was written whose kind differs from the kind the key was registered with.</summary>
public sealed class ValueKindMismatchException : DomainException
{
    public ValueKindMismatchException(ConfigKeyName key, ConfigValueKind expected, ConfigValueKind actual)
        : base($"Key '{key}' is registered as {expected} but was given a {actual} value.")
    {
        Key = key;
        Expected = expected;
        Actual = actual;
    }

    public ConfigKeyName Key { get; }

    public ConfigValueKind Expected { get; }

    public ConfigValueKind Actual { get; }
}
