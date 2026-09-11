using org.g14.Configurino.Domain.ValueObjects;

namespace org.g14.Configurino.Domain.Entities;

public abstract class AbstractPrincipal
{
    /// <summary>
    /// A unique identifier of the principal
    /// </summary>
    public PrincipalId PrincipalId { get; private set; } = PrincipalId.Empty;

    public string DisplayName { get; set; }
}

public class ApplicationClientPrincipal : AbstractPrincipal
{
    /// <summary>
    /// Hash of the API key used by the application
    /// </summary>
    public string ApiKeyHash { get; set; }
}

public class ActiveDirectoryUserPrincipal : AbstractPrincipal
{
    /// <summary>
    /// Active Directory login, including the domain name
    /// </summary>
    public string ActiveDirectoryLogin { get; set; }
}