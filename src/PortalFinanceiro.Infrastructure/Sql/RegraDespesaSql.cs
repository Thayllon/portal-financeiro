namespace PortalFinanceiro.Infrastructure.Sql;

internal static class RegraDespesaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}RegraDespesa";
    static string C => "Id, IdUsuario, Descricao, Valor, Dia, DiaUtil, IdCategoria, IdConta, DataInicio, DataFim, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Descricao, {T}.Valor, {T}.Dia, {T}.DiaUtil, {T}.IdCategoria, {T}.IdConta, {T}.DataInicio, {T}.DataFim, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        cb.Nome AS Conta,
        cat.Nome AS Categoria";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}ContaBancaria cb ON {T}.IdConta = cb.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}CategoriaDespesa cat ON {T}.IdCategoria = cat.Id";
    public static string ObterPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdUsuario = @IdUsuario AND {T}.Ativo = 1 ORDER BY {T}.Descricao";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Descricao, @Valor, @Dia, @DiaUtil, @IdCategoria, @IdConta, @DataInicio, @DataFim, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Descricao = @Descricao, Valor = @Valor, Dia = @Dia, DiaUtil = @DiaUtil, IdCategoria = @IdCategoria, IdConta = @IdConta, DataInicio = @DataInicio, DataFim = @DataFim, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
