using org.g14.Configurino.Application.Access;
using org.g14.Configurino.Application.Access.GetAccessList;
using org.g14.Configurino.Application.Access.GetEffectiveRights;
using org.g14.Configurino.Application.Access.SetRole;
using org.g14.Configurino.Application.Config.ClearValues;
using org.g14.Configurino.Application.Config.GetConfigSnapshot;
using org.g14.Configurino.Application.Config.GetValue;
using org.g14.Configurino.Application.Config.RegisterKeys;
using org.g14.Configurino.Application.Config.Rollback;
using org.g14.Configurino.Application.Config.SetValues;
using org.g14.Configurino.Application.History.GetChangeHistory;
using org.g14.Configurino.Application.History.GetChangeSet;
using org.g14.Configurino.Application.Nodes;
using org.g14.Configurino.Application.Tree.CreateConfigNode;
using org.g14.Configurino.Application.Tree.CreateGroupingNode;
using org.g14.Configurino.Application.Tree.EnsureRoot;
using org.g14.Configurino.Application.Tree.GetChildren;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Config;
using org.g14.Configurino.Domain.ConfigTree.Nodes;

namespace org.g14.Configurino.Application.Tests;

/// <summary>
/// Everything a test needs wired together over one in-memory store, plus canned values so a test only
/// spells out what it is actually about.
/// </summary>
public sealed class World
{
    /// <summary>A person, the first to use the application, and so owner of the whole tree.</summary>
    public static readonly ActorId Human = ActorId.Parse("Leon");

    public static readonly ActorId Client = ActorId.Parse("billing-api@v1");

    /// <summary>Someone nobody has granted anything.</summary>
    public static readonly ActorId Stranger = ActorId.Parse("Mallory");

    public static readonly DateTimeOffset Start = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    public World()
    {
        Store = new InMemoryStore();
        Time = new FixedTimeProvider(Start);
        Guard = new AccessGuard(Store, Store);

        var locator = new NodeLocator(Store, Store, Store);

        EnsureRoot = new EnsureRootHandler(Store, Store, Time);
        CreateGroupingNode = new CreateGroupingNodeHandler(locator, Guard, Store, Time);
        CreateConfigNode = new CreateConfigNodeHandler(locator, Guard, Store, Time);
        GetChildren = new GetChildrenHandler(locator, Guard, Store);
        RegisterKeys = new RegisterKeysHandler(locator, Guard, Store, Time);
        SetValues = new SetValuesHandler(locator, Guard, Store, Time);
        ClearValues = new ClearValuesHandler(locator, Guard, Store, Time);
        Rollback = new RollbackHandler(locator, Guard, Store, Store, Time);
        GetConfigSnapshot = new GetConfigSnapshotHandler(locator, Guard);
        GetValue = new GetValueHandler(locator, Guard);
        GetChangeHistory = new GetChangeHistoryHandler(locator, Guard, Store);
        GetChangeSet = new GetChangeSetHandler(locator, Guard, Store);
        SetRole = new SetRoleHandler(locator, Guard, Store, Time);
        GetAccessList = new GetAccessListHandler(locator, Guard, Store, Store);
        GetEffectiveRights = new GetEffectiveRightsHandler(locator, Guard);
    }

    public InMemoryStore Store { get; }

    public FixedTimeProvider Time { get; }

    public AccessGuard Guard { get; }

    public EnsureRootHandler EnsureRoot { get; }

    public CreateGroupingNodeHandler CreateGroupingNode { get; }

    public CreateConfigNodeHandler CreateConfigNode { get; }

    public GetChildrenHandler GetChildren { get; }

    public RegisterKeysHandler RegisterKeys { get; }

    public SetValuesHandler SetValues { get; }

    public ClearValuesHandler ClearValues { get; }

    public RollbackHandler Rollback { get; }

    public GetConfigSnapshotHandler GetConfigSnapshot { get; }

    public GetValueHandler GetValue { get; }

    public GetChangeHistoryHandler GetChangeHistory { get; }

    public GetChangeSetHandler GetChangeSet { get; }

    public SetRoleHandler SetRole { get; }

    public GetAccessListHandler GetAccessList { get; }

    public GetEffectiveRightsHandler GetEffectiveRights { get; }

    public static NodePath Path(string value) => NodePath.Parse(value);

    public static NodeName Name(string value) => NodeName.Parse(value);

    public static EntryKey Key(string value) => EntryKey.Parse(value);

    public static KeyRegistration Declare(string name, ValueKind kind) => new(Key(name), kind);

    public static KeyAssignment Assign(string name, string value) => new(Key(name), EntryValue.Of(value));

    /// <summary><see cref="Human"/> using the application first, which creates the root and makes them its owner.</summary>
    public static EnsureRootCommand StartUp { get; } = new(Human);

    /// <summary>A world with just the root, owned by <see cref="Human"/>.</summary>
    public static async Task<World> WithRoot()
    {
        var world = new World();

        await world.EnsureRoot.Handle(StartUp);

        return world;
    }

    /// <summary>
    /// A root with a <c>/prod</c> group holding an empty <c>/prod/billing</c> config node, which
    /// <see cref="Client"/> may read and register keys on.
    /// </summary>
    public static async Task<World> WithBillingNode()
    {
        var world = await WithRoot();

        await world.CreateGroupingNode.Handle(new CreateGroupingNodeCommand(NodePath.Root, Name("prod"), Human));
        await world.CreateConfigNode.Handle(new CreateConfigNodeCommand(Path("/prod"), Name("billing"), Human));
        await world.SetRole.Handle(new SetRoleCommand(Billing, Client, Role.ClientApp, Human));

        return world;
    }

    public static NodePath Billing { get; } = Path("/prod/billing");
}
