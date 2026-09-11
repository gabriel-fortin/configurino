using org.g14.Configurino.Domain.Entities;
using org.g14.Configurino.Domain.ValueObjects;

namespace org.g14.Configurino.Domain.Interfaces;

public interface IPrincipalRepository
{
    Task<AbstractPrincipal> GetById(PrincipalId principalId);

    Task<ApplicationClientPrincipal> CreateApplication(string displayName);
    
    Task<ActiveDirectoryUserPrincipal> CreateUser(string displayName);
}