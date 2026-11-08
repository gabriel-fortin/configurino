using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Nodes;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Access.SetRole;

public sealed class SetRoleHandler
{
    /// <summary>
    /// How many times the whole load-apply-save is tried. Setting a role gives the same result however
    /// often it is applied, so trying again after a conflict is always safe; the bound only stops a
    /// pathological pile-up from spinning forever.
    /// </summary>
    public const int MaxAttempts = 3;

    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly INodeAccessListRepository _accessLists;
    private readonly TimeProvider _time;

    public SetRoleHandler(
        NodeLocator locator,
        AccessGuard access,
        INodeAccessListRepository accessLists,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(accessLists);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _access = access;
        _accessLists = accessLists;
        _time = time;
    }

    /// <returns>
    /// The version the node's access list is at once the role has been set; 0 when the node has none,
    /// as nothing was ever set there and <see cref="Role.NoAccess"/> has nothing to take away.
    /// </returns>
    public async Task<int> Handle(SetRoleCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        NodeId nodeId = await _locator.ResolveId(command.Path, cancellationToken);
        await _access.Demand(command.By, nodeId, command.Path, NodeRight.ManagePermissions, cancellationToken);

        for (var attempt = 1; ; attempt++)
        {
            NodeAccessList? stored = await _accessLists.Find(nodeId, cancellationToken);
            NodeAccessList list = stored ?? NodeAccessList.For(nodeId);
            int versionBefore = list.Version;

            list.SetRole(command.Subject, command.Role, command.By, _time.GetUtcNow());

            if (list.Version == versionBefore)
            {
                return list.Version;
            }

            try
            {
                if (stored is null)
                {
                    await _accessLists.Add(list, cancellationToken);
                }
                else
                {
                    await _accessLists.Save(list, cancellationToken);
                }

                return list.Version;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                // someone else changed the list, or created it first; start again from what is stored now
            }
        }
    }
}
