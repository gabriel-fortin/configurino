using org.g14.Configurino.Domain.Enums;

namespace org.g14.Configurino.Domain.ValueObjects;

/// <summary>
/// An optimisation allowing to avoid looking up all ancestors
/// </summary>
public record EffectivePermission(
    PrincipalId PrincipalId,
    AccessRights AccessRights
);