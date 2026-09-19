namespace org.g14.Configurino.Domain.Abstractions;

/// <summary>
/// Base for the aggregate roots: it carries the version and collects the events raised while the
/// aggregate is being changed.
/// </summary>
/// <remarks>
/// <see cref="Version"/> does three jobs at once. It orders the changes made to an aggregate, it is the
/// token a repository compares to detect a concurrent write, and it is what a client application can
/// send back as an ETag to ask "has my configuration changed since?".
/// </remarks>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> domainEvents = [];

    protected AggregateRoot(int version)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(version);

        Version = version;
    }

    public int Version { get; private set; }

    /// <summary>
    /// Hands over the events raised so far and forgets them, so a second call yields nothing.
    /// </summary>
    /// <remarks>
    /// The only way to get at them: taking the events and clearing them is one step, so they cannot be
    /// read, dispatched, and then left behind to be dispatched a second time.
    /// </remarks>
    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        var dequeued = domainEvents.ToArray();
        domainEvents.Clear();
        return dequeued;
    }

    /// <summary>
    /// Records that the aggregate actually changed. Every mutation goes through here, so a version bump
    /// can never be forgotten and an operation that turned out to be a no-op can simply not call it.
    /// </summary>
    protected void MarkChanged() => Version++;

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Guards the timestamps callers pass in. Local times in an audit trail are indistinguishable from
    /// UTC ones once stored, so they are refused at the door rather than normalised silently.
    /// </summary>
    protected static DateTimeOffset RequireUtc(DateTimeOffset at, string parameterName) =>
        at.Offset == TimeSpan.Zero
            ? at
            : throw new ArgumentException("A timestamp recorded by the domain must be in UTC.", parameterName);
}
