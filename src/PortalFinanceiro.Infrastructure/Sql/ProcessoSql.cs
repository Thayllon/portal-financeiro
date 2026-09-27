namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ProcessoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Processo";
    static string TE => $"{SqlDialect.Current.SchemaPrefix}ProcessoEtapa";
    static string C => "Id, IdUsuario, Nome, Descricao, IdParceria, IdContrato, Ativo, DataCadastro, DataAlteracao";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Parceria par ON {T}.IdParceria = par.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Contrato con ON {T}.IdContrato = con.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa parCli ON par.IdCliente = parCli.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa conCli ON con.IdCliente = conCli.Id";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Nome, {T}.Descricao, {T}.IdParceria, {T}.IdContrato, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        CASE WHEN {T}.IdParceria IS NOT NULL THEN 'Parceria' ELSE 'Contrato' END AS VinculoTipo,
        COALESCE(par.Nome, con.Nome) AS VinculoNome,
        COALESCE(parCli.Nome, conCli.Nome) AS Cliente,
        (SELECT COUNT(*) FROM {TE} WHERE IdProcesso = {T}.Id) AS TotalEtapas,
        (SELECT COUNT(*) FROM {TE} WHERE IdProcesso = {T}.Id AND Concluida = {SqlDialect.Current.BooleanTrue}) AS EtapasConcluidas";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdUsuario = @IdUsuario AND (@Ativo IS NULL OR {T}.Ativo = @Ativo) ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Descricao, @IdParceria, @IdContrato, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Descricao = @Descricao, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string ExcluirProcesso => $"DELETE FROM {T} WHERE Id = @Id";
    public static string ContarAtivosPorParceria => $"SELECT COUNT(*) FROM {T} WHERE IdParceria = @IdParceria AND Ativo = {SqlDialect.Current.BooleanTrue}";
    public static string ContarAtivosPorContrato => $"SELECT COUNT(*) FROM {T} WHERE IdContrato = @IdContrato AND Ativo = {SqlDialect.Current.BooleanTrue}";

    static string CE => "Id, IdProcesso, Nome, Descricao, Ordem, Concluida, DataPrevista, DataConclusao, DataCadastro, DataAlteracao";

    public static string ObterEtapaPorId => $"SELECT {CE} FROM {TE} WHERE Id = @Id";
    public static string ListarEtapas => $"SELECT {CE} FROM {TE} WHERE IdProcesso = @IdProcesso ORDER BY Ordem";
    public static string ProximaOrdem => $"SELECT COALESCE(MAX(Ordem), 0) + 1 FROM {TE} WHERE IdProcesso = @IdProcesso";
    public static string ContarEtapasPendentes => $"SELECT COUNT(*) FROM {TE} WHERE IdProcesso = @IdProcesso AND Concluida = {SqlDialect.Current.BooleanFalse}";
    public static string InserirEtapa => $"INSERT INTO {TE} ({CE}) VALUES (@Id, @IdProcesso, @Nome, @Descricao, @Ordem, @Concluida, @DataPrevista, @DataConclusao, @DataCadastro, @DataAlteracao)";
    public static string AtualizarEtapa => $"UPDATE {TE} SET Nome = @Nome, Descricao = @Descricao, Ordem = @Ordem, Concluida = @Concluida, DataPrevista = @DataPrevista, DataConclusao = @DataConclusao, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string ExcluirEtapa => $"DELETE FROM {TE} WHERE Id = @Id";
}
