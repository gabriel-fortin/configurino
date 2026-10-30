using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.GetConfigSnapshot;

/// <summary>
/// Reads the whole configuration at <see cref="Path"/>. A client that already holds a snapshot passes its
/// version as <see cref="KnownVersion"/> and gets a cheap "not modified" when nothing has moved.
/// </summary>
public sealed record GetConfigSnapshotQuery(NodePath Path, int? KnownVersion = null);
