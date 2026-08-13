namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ContaBancariaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ContaBancaria";
    static string C => "Id, IdUsuario, Nome, Banco, Tipo, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorUsuario => $"SELECT {C} FROM {T} WHERE IdUsuario = @IdUsuario AND Ativo = 1 ORDER BY Nome";
    public static string ContarReceitas => $"SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdConta = @IdConta AND Ativo = 1";
    public static string ContarDespesas => $"SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdConta = @IdConta AND Ativo = 1";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Banco, @Tipo, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Banco = @Banco, Tipo = @Tipo, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
