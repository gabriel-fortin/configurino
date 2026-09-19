namespace org.g14.Configurino.Domain.ConfigTree.Nodes;

/// <summary>Which of the two kinds of node this is.</summary>
public enum NodeKind
{
    /// <summary>Groups other nodes of either kind.</summary>
    Grouping = 1,

    /// <summary>Holds the whole configuration of one service. Cannot have children.</summary>
    Config = 2,
}
