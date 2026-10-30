using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.SetValues;

/// <summary>A person gives values to one or more keys of the config node at <see cref="Path"/>, as one change.</summary>
public sealed record SetValuesCommand(
    NodePath Path,
    IReadOnlyCollection<KeyAssignment> Assignments,
    ActorId By,
    string? Note = null);
