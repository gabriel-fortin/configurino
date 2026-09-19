namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>
/// Why a change happened. Without it a rollback is indistinguishable from someone typing the old value
/// back in by hand, which is exactly the question an audit trail exists to answer.
/// </summary>
public abstract record ChangeReason
{
    private protected ChangeReason()
    {
    }

    /// <summary>A person changed the value, optionally saying why.</summary>
    public static ChangeReason Manual(string? note = null) => new ManualChange(note);

    /// <summary>A person put back what an earlier change set had changed.</summary>
    public static ChangeReason Rollback(ChangeSetId target) => new RollbackChange(target);

    /// <summary>A client application's registration changed the schema.</summary>
    public static ChangeReason Registration { get; } = new RegistrationChange();
}

/// <inheritdoc cref="ChangeReason.Manual"/>
public sealed record ManualChange : ChangeReason
{
    public ManualChange(string? note) => Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

    public string? Note { get; }

    public override string ToString() => Note is null ? "manual" : $"manual: {Note}";
}

/// <inheritdoc cref="ChangeReason.Rollback"/>
public sealed record RollbackChange : ChangeReason
{
    public RollbackChange(ChangeSetId target)
    {
        ArgumentNullException.ThrowIfNull(target);

        Target = target;
    }

    /// <summary>The change set being undone.</summary>
    public ChangeSetId Target { get; }

    public override string ToString() => $"rollback of {Target}";
}

/// <inheritdoc cref="ChangeReason.Registration"/>
public sealed record RegistrationChange : ChangeReason
{
    public override string ToString() => "registration";
}
