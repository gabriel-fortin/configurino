namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// Groups everything one operation changed. A person editing five keys and pressing save produces one
/// change set, which is what makes "undo that change" a meaningful request.
/// </summary>
public sealed record ChangeSetId
{
    private ChangeSetId(Guid value) => Value = value;

    public Guid Value { get; }

    /// <summary>Version 7, so change sets sort by when they happened without trusting a clock value.</summary>
    public static ChangeSetId New() => new(Guid.CreateVersion7());

    public static ChangeSetId From(Guid value) =>
        value == Guid.Empty
            ? throw new ArgumentException("A change set id cannot be empty.", nameof(value))
            : new ChangeSetId(value);

    public override string ToString() => Value.ToString();
}
