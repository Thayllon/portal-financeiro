namespace PortalFinanceiro.Infrastructure.Sql;

internal static class CategoriaDespesaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}CategoriaDespesa";
    static string C => "Id, IdUsuario, Nome, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorUsuario => $"SELECT {C} FROM {T} WHERE IdUsuario = @IdUsuario ORDER BY Nome";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
