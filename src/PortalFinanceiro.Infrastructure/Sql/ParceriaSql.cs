namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ParceriaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Parceria";
    static string C => "Id, IdUsuario, IdParceiro, IdCliente, Valor, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.IdParceiro, {T}.IdCliente, {T}.Valor, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        par.Nome AS Parceiro,
        cli.Nome AS Cliente";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa par ON {T}.IdParceiro = par.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa cli ON {T}.IdCliente = cli.Id";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdUsuario = @IdUsuario AND {T}.Ativo = 1 ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @IdParceiro, @IdCliente, @Valor, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET IdParceiro = @IdParceiro, IdCliente = @IdCliente, Valor = @Valor, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string SomarReceitas => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdParceria = @IdParceria AND Ativo = 1 AND Status = @Status";
    public static string SomarDespesas => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdParceria = @IdParceria AND Ativo = 1 AND Status = @Status";
}
