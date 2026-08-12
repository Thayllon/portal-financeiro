namespace PortalFinanceiro.Infrastructure.Sql;

internal static class CategoriaHistoricoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}CategoriaHistorico";
    static string C => "Id, IdCategoria, TipoCategoria, IdUsuario, Acao, NomeAntigo, NomeNovo, CategoriaPaiIdAntiga, CategoriaPaiIdNova, DataCadastro";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdCategoria, @TipoCategoria, @IdUsuario, @Acao, @NomeAntigo, @NomeNovo, @CategoriaPaiIdAntiga, @CategoriaPaiIdNova, @DataCadastro)";
    public static string ListarPorCategoria => $"SELECT {C} FROM {T} WHERE IdCategoria = @IdCategoria AND TipoCategoria = @TipoCategoria ORDER BY DataCadastro";
}
