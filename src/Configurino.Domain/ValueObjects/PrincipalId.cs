namespace org.g14.Configurino.Domain.ValueObjects;

public record struct PrincipalId(Guid Id)
{
    public static PrincipalId New() => new(Guid.NewGuid());
    public static PrincipalId Empty { get; } = new(Guid.Empty);
}