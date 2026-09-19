using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Repositories;

/// <summary>Stores and retrieves config nodes.</summary>
public interface IConfigNodeRepository
{
    /// <summary>Loads a config node, or <c>null</c> when there is none with that id.</summary>
    Task<ConfigNode?> Find(NodeId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a newly created config node. Throws
    /// <see cref="Exceptions.NodeNameAlreadyUsedException"/> if a sibling already has that name.
    /// </summary>
    Task Add(ConfigNode node, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a changed config node, and in the same transaction appends the change log entries its events
    /// describe. Both together or neither: an audit trail written afterwards loses records whenever a
    /// process dies at the wrong moment.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="Exceptions.ConcurrencyConflictException"/> when the stored version has moved on.
    /// Registering keys is idempotent, so retrying the whole load-apply-save is the right response — which
    /// matters when a fleet of pods deploys at once.
    /// </remarks>
    Task Save(ConfigNode node, CancellationToken cancellationToken = default);
}
