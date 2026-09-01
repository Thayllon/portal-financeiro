namespace PortalFinanceiro.Infrastructure.Sql;

internal static class PessoaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Pessoa";
    static string C => "Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorUsuario => $"SELECT {C} FROM {T} WHERE IdUsuario = @IdUsuario AND Ativo = 1 ORDER BY Nome";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Telefone, @Tipo, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Telefone = @Telefone, Tipo = @Tipo, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}