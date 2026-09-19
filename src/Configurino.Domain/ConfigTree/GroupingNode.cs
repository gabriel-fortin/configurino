using org.g14.Configurino.Domain.Abstractions;
using org.g14.Configurino.Domain.Access;
using org.g14.Configurino.Domain.ConfigTree.Events;

namespace org.g14.Configurino.Domain.ConfigTree;

/// <summary>
/// A node that groups other nodes. Purely organisational: it holds no configuration of its own and
/// nothing is inherited through it.
/// </summary>
/// <remarks>
/// <para>
/// The aggregate is just this node. It does not hold its children — they are found by asking
/// <c>INodeDirectory</c> for everything one level below, which keeps creating a node a single-aggregate
/// write and keeps a group with thousands of children loadable.
/// </para>
/// </remarks>
public sealed class GroupingNode : AggregateRoot
{
    private GroupingNode(NodeId id, NodeId? parentId, NodeName? name)
        : base(version: 0)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
    }

    public NodeId Id { get; }

    /// <summary><c>null</c> only for the root, the one node that sits under nothing.</summary>
    public NodeId? ParentId { get; }

    /// <summary><c>null</c> only for the root, which has no name of its own.</summary>
    public NodeName? Name { get; }

    public bool IsRoot => ParentId is null;

    /// <summary>
    /// Creates the grouping node that exists by default. Its id is fixed, so a Service layer that calls
    /// this whenever it finds no root ends up with exactly one however many times it runs.
    /// </summary>
    public static GroupingNode CreateRoot(ActorId by, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(by);

        var root = new GroupingNode(NodeId.Root, parentId: null, name: null);
        root.MarkChanged();
        root.Raise(new GroupingNodeCreated(root.Id, null, null, by, RequireUtc(at, nameof(at))));

        return root;
    }

    /// <summary>Creates a grouping node under this one.</summary>
    public GroupingNode CreateChildGroup(NodeName name, ActorId by, DateTimeOffset at)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(by);

        var child = new GroupingNode(NodeId.New(), Id, name);
        child.MarkChanged();
        child.Raise(new GroupingNodeCreated(child.Id, Id, name, by, RequireUtc(at, nameof(at))));

        return child;
    }

    /// <summary>
    /// Creates a config node under this one. It starts empty: only a client application's registration
    /// puts keys in it.
    /// </summary>
    public ConfigNode CreateChildConfig(NodeName name, ActorId by, DateTimeOffset at) =>
        ConfigNode.CreateUnder(this, name, by, at);

    public override string ToString() => IsRoot ? "/" : $"{Name} ({Id})";
}
