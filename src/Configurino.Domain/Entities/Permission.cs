using org.g14.Configurino.Domain.Enums;
using org.g14.Configurino.Domain.ValueObjects;

namespace org.g14.Configurino.Domain.Entities;

// child entity for Nodes
public record class Permission
{
    public PrincipalId PrincipalId { get; init; }
    public AccessRights AccessRights { get; init; }
    
    protected Permission(PrincipalId PrincipalId, AccessRights AccessRights)
    {
        this.PrincipalId = PrincipalId;
        this.AccessRights = AccessRights;
    }

    public static Permission Create(PrincipalId principalId, AccessRights accessRights)
    {
        return new(principalId, accessRights);
    }
}