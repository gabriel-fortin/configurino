using org.g14.Configurino.Domain.Enums;
using org.g14.Configurino.Domain.ValueObjects;

namespace org.g14.Configurino.Domain.Entities;

public abstract class AbstractConfigNode
{
    public int Id { get; set; }  // TODO: Remove setter but be wary that this might upset EF Core
    
    public string Name { get; set; }

    private List<Permission> _permissions = new();
    public IReadOnlyList<Permission> Permissions => _permissions;

    private List<AbstractConfigNode> _children = new();
    public IReadOnlyList<AbstractConfigNode> Children => _children;

    // public NodeType Type { get; set; }

    public Task AddPermission(PrincipalId principalId, AccessRights accessRights)
    {
        var permission = Permission.Create(principalId, accessRights);
        _permissions.Add(permission);
        _children.ForEach(child => child.AddEffectivePermission(permission));
        return Task.CompletedTask;
    }
    
    // effective permissions are an optimisation allowing to avoid looking up all ancestors
    // it's a summary of applying all permissions from higher up in the tree
    protected Task AddEffectivePermission(Permission permission)
    {
        throw new NotImplementedException();
    }

    public Task RemovePermission()
    {
        throw new NotImplementedException();
    }

    public Task AddChild()
    {
        throw new NotImplementedException();
    }

    public Task RemoveChild()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// A folder in the config tree structure.
/// It exists for ease of management; it does not represent actual config values or names
/// </summary>
public class TreeGrouping : AbstractConfigNode
{
    
}

/// <summary>
/// An object config node – a node that exists in some appsettings file as an object
/// </summary>
public class ConfigObject : AbstractConfigNode
{
    
}

/// <summary>
/// An array config node – a node that exists in some appsettings file as an array
/// </summary>
public class ConfigArray : AbstractConfigNode
{
    
}

/// <summary>
/// A value config node – a node that exists in some appsettings file and has a value
/// </summary>
public class ConfigLeaf : AbstractConfigNode
{
    /// Used only for nodes of type <see cref="NodeType.Leaf"/>
    public string Value
    {
        get;
        set;
        // get => Type == NodeType.Leaf
        //     ? field
        //     : throw new DomainException($"Node {Id}/{Name} is of type {Type}, so it has no value.");
        // set => field = (Type == NodeType.Leaf)
        //     ? value
        //     : throw new DomainException($"Node {Id}/{Name} is of type {Type}, so it has no value.");
    }
    
}