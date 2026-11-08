namespace org.g14.Configurino.Domain.Access;

/// <summary>
/// Works out what an actor may do on a node from the access lists of the node and of every node above it.
/// </summary>
/// <remarks>
/// The whole rule set, kept in one place: rights only ever add up. A permission on a node reaches every
/// node below it, nothing below can take it away, and there is no such thing as a denial. So the order of
/// the lists does not matter, and a missing list simply adds nothing.
/// </remarks>
public static class AccessEvaluator
{
    /// <param name="actor">Whose rights to work out.</param>
    /// <param name="lineage">The lists of the node and its ancestors — those that exist, in any order.</param>
    public static NodeRight EffectiveRights(ActorId actor, IEnumerable<NodeAccessList> lineage)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(lineage);

        var rights = NodeRight.None;

        foreach (var list in lineage)
        {
            rights |= list.RightsOf(actor);
        }

        return rights;
    }
}
