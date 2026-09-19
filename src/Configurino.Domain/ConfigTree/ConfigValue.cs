namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// A value held against a registered key, carrying its own type so it can always be checked against
/// the kind the key was registered with.
/// </summary>
/// <remarks>
/// The hierarchy is closed by a <c>private protected</c> constructor, so no type outside this assembly
/// can extend it and a <c>switch</c> over the three cases is exhaustive in practice. Values of
/// different kinds are never equal, because records compare their type before their contents.
/// </remarks>
public abstract record ConfigValue
{
    private protected ConfigValue()
    {
    }

    public abstract ConfigValueKind Kind { get; }

    public static StringValue Of(string value) => new(value);

    public static IntegerValue Of(long value) => new(value);

    public static BooleanValue Of(bool value) => new(value);
}

/// <summary>Text, stored as given — no trimming, no canonicalisation.</summary>
public sealed record StringValue : ConfigValue
{
    public StringValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
    }

    public string Value { get; }

    public override ConfigValueKind Kind => ConfigValueKind.String;

    public override string ToString() => Value;
}

/// <summary>
/// A whole number. 64-bit because configuration routinely holds milliseconds, byte sizes and epoch
/// seconds, all of which outgrow 32 bits.
/// </summary>
public sealed record IntegerValue : ConfigValue
{
    public IntegerValue(long value) => Value = value;

    public long Value { get; }

    public override ConfigValueKind Kind => ConfigValueKind.Integer;

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}

/// <summary>A flag.</summary>
public sealed record BooleanValue : ConfigValue
{
    public BooleanValue(bool value) => Value = value;

    public bool Value { get; }

    public override ConfigValueKind Kind => ConfigValueKind.Boolean;

    public override string ToString() => Value ? "true" : "false";
}
