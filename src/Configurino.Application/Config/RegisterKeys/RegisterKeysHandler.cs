using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.Exceptions;
using org.g14.Configurino.Domain.Repositories;

namespace org.g14.Configurino.Application.Config.RegisterKeys;

public sealed class RegisterKeysHandler
{
    /// <summary>
    /// How many times the whole load-apply-save is tried. Registration is idempotent, so trying again after
    /// a conflict is always safe; the bound only stops a pathological pile-up from spinning forever.
    /// </summary>
    public const int MaxAttempts = 3;

    private readonly NodeLocator _locator;
    private readonly AccessGuard _access;
    private readonly IConfigNodeRepository _configNodes;
    private readonly TimeProvider _time;

    public RegisterKeysHandler(
        NodeLocator locator,
        AccessGuard access,
        IConfigNodeRepository configNodes,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(locator);
        ArgumentNullException.ThrowIfNull(access);
        ArgumentNullException.ThrowIfNull(configNodes);
        ArgumentNullException.ThrowIfNull(time);

        _locator = locator;
        _access = access;
        _configNodes = configNodes;
        _time = time;
    }

    /// <returns>The version the node is at once the registration has been applied.</returns>
    public async Task<int> Handle(RegisterKeysCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var nodeId = await _locator.ResolveConfigId(command.Path, cancellationToken);
        await _access.Demand(command.By, nodeId, command.Path, NodeRight.RegisterKeys, cancellationToken);

        for (var attempt = 1; ; attempt++)
        {
            var node = await _locator.LoadConfigNode(command.Path, cancellationToken);
            var versionBefore = node.Version;

            node.RegisterKeys(command.Keys, command.By, _time.GetUtcNow());

            if (node.Version == versionBefore)
            {
                return node.Version;
            }

            try
            {
                await _configNodes.Save(node, cancellationToken);

                return node.Version;
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                // someone else moved the node on; start again from what is stored now
            }
        }
    }
}
