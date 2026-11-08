using static org.g14.Configurino.Domain.Access.NodeRight;

namespace org.g14.Configurino.Domain.Access;

public class Role
{
    public NodeRight Rights { get; }
    
    private Role(NodeRight rights)
    {
        Rights = rights;
    }

    public static readonly Role NoAccess = new(None);
    public static readonly Role Reader = new(Browse | ReadConfig | ReadHistory);
    public static readonly Role Editor = new(Browse | ReadConfig | ReadHistory | EditValues);
    public static readonly Role Owner = new(Browse | ReadConfig | ReadHistory | EditValues | CreateNodes | ManagePermissions);
    public static readonly Role ClientApp = new(ReadConfig | RegisterKeys);
}