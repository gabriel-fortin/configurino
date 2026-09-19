namespace org.g14.Configurino.Domain.Abstractions;

/// <summary>
/// Something that happened to an aggregate. Raised while the aggregate is being changed and dispatched
/// by the Service layer once the change has been committed.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the change happened, in UTC. Supplied by the caller, never read from a clock.</summary>
    DateTimeOffset OccurredAt { get; }
}
