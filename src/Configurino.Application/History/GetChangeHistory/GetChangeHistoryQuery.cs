using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Changes;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.History.GetChangeHistory;

/// <summary>
/// Reads a page of the history of the config node at <see cref="Path"/>, newest first.
/// <see cref="Before"/> continues after the last change set of the previous page.
/// </summary>
public sealed record GetChangeHistoryQuery
{
    private const int MaxLimit = 500;

    public GetChangeHistoryQuery(NodePath path, ActorId by, int limit, ChangeSetId? before = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(by);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(limit, MaxLimit);

        Path = path;
        By = by;
        Limit = limit;
        Before = before;
    }

    public NodePath Path { get; }

    public ActorId By { get; }

    public int Limit { get; }

    public ChangeSetId? Before { get; }
}
