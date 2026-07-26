namespace PortalFinanceiro.Infrastructure.Sql;

internal static class DespesaRecorrenteSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}DespesaRecorrente";
    static string C => "Id, IdUsuario, Descricao, Valor, Dia, IdCategoria, IdConta, DataInicio, DataFim, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorUsuario => $"SELECT {C} FROM {T} WHERE IdUsuario = @IdUsuario ORDER BY Descricao";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Descricao, @Valor, @Dia, @IdCategoria, @IdConta, @DataInicio, @DataFim, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Descricao = @Descricao, Valor = @Valor, Dia = @Dia, IdCategoria = @IdCategoria, IdConta = @IdConta, DataInicio = @DataInicio, DataFim = @DataFim, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
