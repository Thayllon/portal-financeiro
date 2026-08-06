namespace PortalFinanceiro.Infrastructure.Sql;

internal static class CategoriaDespesaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}CategoriaDespesa";
    static string C => "Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorUsuario => $"SELECT {C} FROM {T} WHERE IdUsuario = @IdUsuario AND Ativo = 1 ORDER BY Nome";
    public static string ListarPorPai => $"SELECT {C} FROM {T} WHERE CategoriaPaiId = @CategoriaPaiId AND Ativo = 1";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @CategoriaPaiId, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, CategoriaPaiId = @CategoriaPaiId, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string Excluir => $"UPDATE {T} SET Ativo = 0, DataAlteracao = GETUTCDATE() WHERE Id = @Id";
}
