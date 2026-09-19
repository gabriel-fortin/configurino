using org.g14.Configurino.Domain.Access;

namespace org.g14.Configurino.Domain.ConfigTree.Changes;

/// <summary>
/// Who last touched a key and when. Enough to show "last changed by" beside every key without going
/// anywhere near the change log, which holds the full story.
/// </summary>
public sealed record ChangeStamp
{
    public ChangeStamp(ActorId by, DateTimeOffset at, int version)
    {
        ArgumentNullException.ThrowIfNull(by);
        ArgumentOutOfRangeException.ThrowIfNegative(version);

        By = by;
        At = at;
        Version = version;
    }

    public ActorId By { get; }

    public DateTimeOffset At { get; }

    /// <summary>The node version this change produced, which orders changes without trusting clocks.</summary>
    public int Version { get; }

    public override string ToString() => $"{By} at {At:O} (v{Version})";
}
