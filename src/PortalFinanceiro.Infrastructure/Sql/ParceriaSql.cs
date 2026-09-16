namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ParceriaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Parceria";
    static string C => "Id, IdUsuario, Nome, IdParceiro, IdCliente, Valor, PercentualParceiro, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Nome, {T}.IdParceiro, {T}.IdCliente, {T}.Valor, {T}.PercentualParceiro, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        par.Nome AS Parceiro,
        cli.Nome AS Cliente";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa par ON {T}.IdParceiro = par.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa cli ON {T}.IdCliente = cli.Id";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdUsuario = @IdUsuario AND {T}.Ativo = 1 ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @IdParceiro, @IdCliente, @Valor, @PercentualParceiro, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, IdParceiro = @IdParceiro, IdCliente = @IdCliente, Valor = @Valor, PercentualParceiro = @PercentualParceiro, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string SomarReceitas => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdParceria = @IdParceria AND Ativo = 1 AND Status = @Status";
    public static string SomarDespesas => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdParceria = @IdParceria AND Ativo = 1 AND Status = @Status";
    public static string SomarReceitasAnual => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdUsuario = @IdUsuario AND Ativo = 1 AND IdParceria IS NOT NULL AND YEAR(Data) = @Ano AND (@IdConta IS NULL OR IdConta = @IdConta) AND Status = @Status";
    public static string SomarDespesasAnual => $"SELECT COALESCE(SUM(Valor),0) FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdUsuario = @IdUsuario AND Ativo = 1 AND IdParceria IS NOT NULL AND YEAR(Data) = @Ano AND (@IdConta IS NULL OR IdConta = @IdConta) AND Status = @Status";
    public static string ContarParceriasAnual => $@"SELECT COUNT(DISTINCT IdParceria) FROM (
        SELECT IdParceria FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdUsuario = @IdUsuario AND Ativo = 1 AND IdParceria IS NOT NULL AND YEAR(Data) = @Ano AND (@IdConta IS NULL OR IdConta = @IdConta)
        UNION
        SELECT IdParceria FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdUsuario = @IdUsuario AND Ativo = 1 AND IdParceria IS NOT NULL AND YEAR(Data) = @Ano AND (@IdConta IS NULL OR IdConta = @IdConta)
    ) AS P";
}
