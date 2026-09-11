using org.g14.Configurino.Domain.Entities;

namespace org.g14.Configurino.Domain.Interfaces;

/// <summary>
/// Interactions with the current Principal
/// </summary>
public interface IPrincipalProvider
{
    Task<AbstractPrincipal> GetCurrentPrincipal();
}