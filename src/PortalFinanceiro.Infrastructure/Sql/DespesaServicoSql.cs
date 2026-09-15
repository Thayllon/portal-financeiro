namespace PortalFinanceiro.Infrastructure.Sql;

internal static class DespesaServicoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}DespesaServico";
    public static string ListarPorDespesaId => $"SELECT Id, DespesaId, CategoriaServicoId, SubcategoriaServicoId FROM {T} WHERE DespesaId = @DespesaId";
    public static string Inserir => $"INSERT INTO {T} (Id, DespesaId, CategoriaServicoId, SubcategoriaServicoId) VALUES (@Id, @DespesaId, @CategoriaServicoId, @SubcategoriaServicoId)";
    public static string ExcluirPorDespesaId => $"DELETE FROM {T} WHERE DespesaId = @DespesaId";
}
