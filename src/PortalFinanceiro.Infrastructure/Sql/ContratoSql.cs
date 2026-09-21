namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ContratoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Contrato";
    static string C => "Id, IdUsuario, Nome, IdCliente, Valor, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Nome, {T}.IdCliente, {T}.Valor, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        cli.Nome AS Cliente";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa cli ON {T}.IdCliente = cli.Id";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdUsuario = @IdUsuario AND (@Ativo IS NULL OR {T}.Ativo = @Ativo) ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @IdCliente, @Valor, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, IdCliente = @IdCliente, Valor = @Valor, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string SomarReceitas => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdContrato = @IdContrato AND Ativo = {SqlDialect.Current.BooleanTrue} AND Status = @Status";
}
