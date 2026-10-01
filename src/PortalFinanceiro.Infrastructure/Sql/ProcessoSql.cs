namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ProcessoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Processo";
    static string TE => $"{SqlDialect.Current.SchemaPrefix}ProcessoEtapa";
    static string TI => $"{SqlDialect.Current.SchemaPrefix}ProcessoEtapaItem";
    static string TA => $"{SqlDialect.Current.SchemaPrefix}ProcessoAnexo";
    static string C => "Id, IdUsuario, Nome, Descricao, IdParceria, IdContrato, IdModeloProcesso, IdCliente, Ativo, DataEncerramento, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Parceria par ON {T}.IdParceria = par.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Contrato con ON {T}.IdContrato = con.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa parCli ON par.IdCliente = parCli.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa conCli ON con.IdCliente = conCli.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa cli ON {T}.IdCliente = cli.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}ModeloProcesso mod ON {T}.IdModeloProcesso = mod.Id";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Nome, {T}.Descricao, {T}.IdParceria, {T}.IdContrato, {T}.IdModeloProcesso, {T}.IdCliente, {T}.Ativo, {T}.DataEncerramento, {T}.DataCadastro, {T}.DataAlteracao, {T}.CriadoPor, {T}.AlteradoPor,
        COALESCE(mod.Nome, '') AS ModeloNome,
        CASE WHEN {T}.IdParceria IS NOT NULL THEN 'Parceria' ELSE 'Contrato' END AS VinculoTipo,
        COALESCE(par.Nome, con.Nome) AS VinculoNome,
        COALESCE(cli.Nome, parCli.Nome, conCli.Nome) AS Cliente,
        COALESCE(faseAtual.Nome, '') AS FaseAtual,
        (SELECT COUNT(*) FROM {TE} WHERE IdProcesso = {T}.Id) AS TotalEtapas,
        (SELECT COUNT(*) FROM {TE} WHERE IdProcesso = {T}.Id AND Concluida = {SqlDialect.Current.BooleanTrue}) AS EtapasConcluidas,
        (SELECT COUNT(*) FROM {TI} WHERE IdProcessoEtapa IN (SELECT Id FROM {TE} WHERE IdProcesso = {T}.Id)) AS TotalItens,
        (SELECT COUNT(*) FROM {TI} WHERE Concluida = {SqlDialect.Current.BooleanTrue} AND IdProcessoEtapa IN (SELECT Id FROM {TE} WHERE IdProcesso = {T}.Id)) AS ItensConcluidos";
    static string FaseAtualJoin => $@"OUTER APPLY (SELECT TOP 1 Nome FROM {TE} WHERE IdProcesso = {T}.Id AND Concluida = {SqlDialect.Current.BooleanFalse} ORDER BY Ordem) AS faseAtual";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComNomes} FROM {T} {Joins} {FaseAtualJoin} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {T} {Joins} {FaseAtualJoin} WHERE {T}.IdUsuario = @IdUsuario AND (@Ativo IS NULL OR {T}.Ativo = @Ativo) ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Descricao, @IdParceria, @IdContrato, @IdModeloProcesso, @IdCliente, @Ativo, @DataEncerramento, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Descricao = @Descricao, Ativo = @Ativo, DataEncerramento = @DataEncerramento, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string ExcluirProcesso => $"DELETE FROM {T} WHERE Id = @Id";
    public static string ContarAtivosPorParceria => $"SELECT COUNT(*) FROM {T} WHERE IdParceria = @IdParceria AND Ativo = {SqlDialect.Current.BooleanTrue}";
    public static string ContarAtivosPorContrato => $"SELECT COUNT(*) FROM {T} WHERE IdContrato = @IdContrato AND Ativo = {SqlDialect.Current.BooleanTrue}";
    public static string ContarPorParceria => $"SELECT COUNT(*) FROM {T} WHERE IdParceria = @IdParceria";
    public static string ContarPorContrato => $"SELECT COUNT(*) FROM {T} WHERE IdContrato = @IdContrato";

    static string CE => "Id, IdProcesso, Nome, Descricao, Ordem, Concluida, DataPrevista, DataInicio, DataConclusao, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";

    public static string ObterEtapaPorId => $"SELECT {CE} FROM {TE} WHERE Id = @Id";
    public static string ListarEtapas => $"SELECT {CE} FROM {TE} WHERE IdProcesso = @IdProcesso ORDER BY Ordem";
    public static string ProximaOrdem => $"SELECT COALESCE(MAX(Ordem), 0) + 1 FROM {TE} WHERE IdProcesso = @IdProcesso";
    public static string ContarEtapasPendentes => $"SELECT COUNT(*) FROM {TE} WHERE IdProcesso = @IdProcesso AND Concluida = {SqlDialect.Current.BooleanFalse}";
    public static string InserirEtapa => $"INSERT INTO {TE} ({CE}) VALUES (@Id, @IdProcesso, @Nome, @Descricao, @Ordem, @Concluida, @DataPrevista, @DataInicio, @DataConclusao, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string AtualizarEtapa => $"UPDATE {TE} SET Nome = @Nome, Descricao = @Descricao, Ordem = @Ordem, Concluida = @Concluida, DataPrevista = @DataPrevista, DataInicio = @DataInicio, DataConclusao = @DataConclusao, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string ExcluirEtapa => $"DELETE FROM {TE} WHERE Id = @Id";

    static string CI => "Id, IdProcessoEtapa, Nome, Descricao, Obrigatorio, ExigeAnexo, Ordem, Concluida, DataInicio, DataConclusao, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";

    public static string ObterItemPorId => $"SELECT {CI} FROM {TI} WHERE Id = @Id";
    public static string ListarItens => $"SELECT {CI} FROM {TI} WHERE IdProcessoEtapa = @IdProcessoEtapa ORDER BY Ordem";
    public static string ProximaOrdemItem => $"SELECT COALESCE(MAX(Ordem), 0) + 1 FROM {TI} WHERE IdProcessoEtapa = @IdProcessoEtapa";
    public static string ContarItensObrigatoriosPendentes => $"SELECT COUNT(*) FROM {TI} WHERE IdProcessoEtapa = @IdProcessoEtapa AND Obrigatorio = {SqlDialect.Current.BooleanTrue} AND Concluida = {SqlDialect.Current.BooleanFalse}";
    public static string ContarAnexosPorItem => $"SELECT COUNT(*) FROM {TA} WHERE IdProcessoEtapaItem = @IdProcessoEtapaItem";
    public static string InserirItem => $"INSERT INTO {TI} ({CI}) VALUES (@Id, @IdProcessoEtapa, @Nome, @Descricao, @Obrigatorio, @ExigeAnexo, @Ordem, @Concluida, @DataInicio, @DataConclusao, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string AtualizarItem => $"UPDATE {TI} SET Nome = @Nome, Descricao = @Descricao, Obrigatorio = @Obrigatorio, ExigeAnexo = @ExigeAnexo, Ordem = @Ordem, Concluida = @Concluida, DataInicio = @DataInicio, DataConclusao = @DataConclusao, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string ExcluirItem => $"DELETE FROM {TI} WHERE Id = @Id";
}
