namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// The outcome of reading one key: it was never registered, it is registered but nobody has given it a
/// value, or it has one.
/// </summary>
/// <remarks>
/// Three outcomes rather than a nullable value, because "no such key" and "no value yet" call for
/// different responses from a client application and are easy to confuse when both arrive as null.
/// </remarks>
public abstract record KeyReadResult
{
    private protected KeyReadResult()
    {
    }

    /// <summary>No client application has ever registered this key here.</summary>
    public static KeyReadResult Unknown { get; } = new UnknownKey();

    /// <summary>The key is registered, but no value has been given to it.</summary>
    public static KeyReadResult NotSet { get; } = new UnsetKey();

    public static KeyReadResult Set(ConfigValue value) => new SetKey(value);
}

/// <inheritdoc cref="KeyReadResult.Unknown"/>
public sealed record UnknownKey : KeyReadResult
{
    public override string ToString() => "unknown";
}

/// <inheritdoc cref="KeyReadResult.NotSet"/>
public sealed record UnsetKey : KeyReadResult
{
    public override string ToString() => "not set";
}

/// <summary>The key is registered and holds a value.</summary>
public sealed record SetKey : KeyReadResult
{
    public SetKey(ConfigValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
    }

    public ConfigValue Value { get; }

    public override string ToString() => Value.ToString();
}
