namespace PortalFinanceiro.Core.Domain.Results;

public sealed class Unit
{
    private Unit() { }
    public static Unit Value { get; } = new();
}
