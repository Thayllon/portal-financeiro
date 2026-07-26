namespace PortalFinanceiro.Core.Domain.Results;

public static class Resultado
{
    public static Result<Unit> Sucesso() => Result<Unit>.Sucesso(Unit.Value);
}
