using org.g14.Configurino.Domain.ConfigTree;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// A config node changed between being loaded and being saved. Registration is idempotent, so a
/// caller retrying the whole load-apply-save cycle is the expected response.
/// </summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(NodeId nodeId, int expectedVersion, int actualVersion)
        : base($"Config node {nodeId} was at version {actualVersion}, not the expected {expectedVersion}.")
    {
        NodeId = nodeId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public NodeId NodeId { get; }

    public int ExpectedVersion { get; }

    public int ActualVersion { get; }
}
