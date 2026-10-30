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
    public static readonly ActorId Human = ActorId.Parse("Leon");
    public static readonly ActorId Client = ActorId.Parse("billing-api@v1");

    public static readonly DateTimeOffset Start = new(2026, 9, 19, 10, 0, 0, TimeSpan.Zero);

    public World()
    {
        Store = new InMemoryStore();
        Time = new FixedTimeProvider(Start);

        var locator = new NodeLocator(Store, Store, Store);

        EnsureRoot = new EnsureRootHandler(Store, Time);
        CreateGroupingNode = new CreateGroupingNodeHandler(locator, Store, Time);
        CreateConfigNode = new CreateConfigNodeHandler(locator, Store, Time);
        GetChildren = new GetChildrenHandler(locator, Store);
        RegisterKeys = new RegisterKeysHandler(locator, Store, Time);
        SetValues = new SetValuesHandler(locator, Store, Time);
        ClearValues = new ClearValuesHandler(locator, Store, Time);
        Rollback = new RollbackHandler(locator, Store, Store, Time);
        GetConfigSnapshot = new GetConfigSnapshotHandler(locator);
        GetValue = new GetValueHandler(locator);
        GetChangeHistory = new GetChangeHistoryHandler(locator, Store);
        GetChangeSet = new GetChangeSetHandler(locator, Store);
    }

    public InMemoryStore Store { get; }

    public FixedTimeProvider Time { get; }

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

    public static NodePath Path(string value) => NodePath.Parse(value);

    public static NodeName Name(string value) => NodeName.Parse(value);

    public static EntryKey Key(string value) => EntryKey.Parse(value);

    public static KeyRegistration Declare(string name, ValueKind kind) => new(Key(name), kind);

    public static KeyAssignment Assign(string name, string value) => new(Key(name), EntryValue.Of(value));

    /// <summary>A root with a <c>/prod</c> group holding an empty <c>/prod/billing</c> config node.</summary>
    public static async Task<World> WithBillingNode()
    {
        var world = new World();

        await world.EnsureRoot.Handle(new EnsureRootCommand(Human));
        await world.CreateGroupingNode.Handle(new CreateGroupingNodeCommand(NodePath.Root, Name("prod"), Human));
        await world.CreateConfigNode.Handle(new CreateConfigNodeCommand(Path("/prod"), Name("billing"), Human));

        return world;
    }

    public static NodePath Billing { get; } = Path("/prod/billing");
}
