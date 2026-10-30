using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Tree;

/// <summary>
/// What creating a node hands back: the identity it was given and the path it can be addressed by.
/// </summary>
public sealed record CreatedNode(NodeId Id, NodePath Path);
