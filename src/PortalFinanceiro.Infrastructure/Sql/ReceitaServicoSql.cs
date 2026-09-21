namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ReceitaServicoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ReceitaServico";
    public static string ListarPorReceitaId => $@"SELECT {T}.Id, {T}.ReceitaId, {T}.CategoriaServicoId, {T}.SubcategoriaServicoId,
        cat.Nome AS CategoriaServico, sub.Nome AS SubcategoriaServico
        FROM {T}
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}CategoriaServico cat ON {T}.CategoriaServicoId = cat.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}CategoriaServico sub ON {T}.SubcategoriaServicoId = sub.Id
        WHERE {T}.ReceitaId = @ReceitaId";
    public static string Inserir => $"INSERT INTO {T} (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId) VALUES (@Id, @ReceitaId, @CategoriaServicoId, @SubcategoriaServicoId)";
    public static string ExcluirPorReceitaId => $"DELETE FROM {T} WHERE ReceitaId = @ReceitaId";
}
