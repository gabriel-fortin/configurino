using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Config.ClearValues;

/// <summary>A person takes the values off one or more keys of the config node at <see cref="Path"/>, as one change.</summary>
public sealed record ClearValuesCommand(
    NodePath Path,
    IReadOnlyCollection<EntryKey> Keys,
    ActorId By,
    string? Note = null);
