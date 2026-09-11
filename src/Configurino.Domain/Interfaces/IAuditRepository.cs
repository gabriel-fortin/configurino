using org.g14.Configurino.Domain.ValueObjects;

namespace org.g14.Configurino.Domain.Interfaces;

public interface IAuditRepository
{
    Task Audit(PrincipalId principalId, int nodeId, string eventDescription);
}