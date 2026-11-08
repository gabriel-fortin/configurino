using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Domain.Exceptions;

/// <summary>
/// An aggregate of a node — its config or its access list — changed between being loaded and being saved.
/// For idempotent operations such as registration or granting, a caller retrying the whole
/// load-apply-save cycle is the expected response.
/// </summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(NodeId nodeId, int expectedVersion, int actualVersion)
        : base($"Node {nodeId} was stored at version {actualVersion}, not the expected {expectedVersion}.")
    {
        NodeId = nodeId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public NodeId NodeId { get; }

    public int ExpectedVersion { get; }

    public int ActualVersion { get; }
}
