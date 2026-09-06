namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ReceitaServicoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ReceitaServico";
    public static string ListarPorReceitaId => $"SELECT Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId FROM {T} WHERE ReceitaId = @ReceitaId";
    public static string Inserir => $"INSERT INTO {T} (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId) VALUES (@Id, @ReceitaId, @CategoriaServicoId, @SubcategoriaServicoId)";
    public static string ExcluirPorReceitaId => $"DELETE FROM {T} WHERE ReceitaId = @ReceitaId";
}
