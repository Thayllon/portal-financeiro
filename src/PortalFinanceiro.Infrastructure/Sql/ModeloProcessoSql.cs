namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ModeloProcessoSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ModeloProcesso";
    static string TE => $"{SqlDialect.Current.SchemaPrefix}ModeloEtapa";
    static string TI => $"{SqlDialect.Current.SchemaPrefix}ModeloItem";
    static string C => "Id, IdUsuario, Nome, Descricao, Ativo, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";
    static string CComTotais => $@"{T}.Id, {T}.IdUsuario, {T}.Nome, {T}.Descricao, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao, {T}.CriadoPor, {T}.AlteradoPor,
        (SELECT COUNT(*) FROM {TE} WHERE IdModeloProcesso = {T}.Id) AS TotalEtapas,
        (SELECT COUNT(*) FROM {TI} WHERE IdModeloEtapa IN (SELECT Id FROM {TE} WHERE IdModeloProcesso = {T}.Id)) AS TotalItens";

    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterProjecaoPorId => $"SELECT {CComTotais} FROM {T} WHERE {T}.Id = @Id";
    public static string ListarPorUsuario => $"SELECT {CComTotais} FROM {T} WHERE {T}.IdUsuario = @IdUsuario AND (@Ativo IS NULL OR {T}.Ativo = @Ativo) ORDER BY {T}.DataCadastro DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Nome, @Descricao, @Ativo, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Descricao = @Descricao, Ativo = @Ativo, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string Excluir => $"DELETE FROM {T} WHERE Id = @Id";

    static string CE => "Id, IdModeloProcesso, Nome, Descricao, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";

    public static string ObterEtapaPorId => $"SELECT {CE} FROM {TE} WHERE Id = @Id";
    public static string ListarEtapas => $"SELECT {CE} FROM {TE} WHERE IdModeloProcesso = @IdModeloProcesso ORDER BY Ordem";
    public static string ProximaOrdemEtapa => $"SELECT COALESCE(MAX(Ordem), 0) + 1 FROM {TE} WHERE IdModeloProcesso = @IdModeloProcesso";
    public static string InserirEtapa => $"INSERT INTO {TE} ({CE}) VALUES (@Id, @IdModeloProcesso, @Nome, @Descricao, @Ordem, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string AtualizarEtapa => $"UPDATE {TE} SET Nome = @Nome, Descricao = @Descricao, Ordem = @Ordem, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string ExcluirEtapa => $"DELETE FROM {TE} WHERE Id = @Id";

    static string CI => "Id, IdModeloEtapa, Nome, Descricao, Obrigatorio, ExigeAnexo, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor";

    public static string ObterItemPorId => $"SELECT {CI} FROM {TI} WHERE Id = @Id";
    public static string ListarItens => $"SELECT {CI} FROM {TI} WHERE IdModeloEtapa = @IdModeloEtapa ORDER BY Ordem";
    public static string ProximaOrdemItem => $"SELECT COALESCE(MAX(Ordem), 0) + 1 FROM {TI} WHERE IdModeloEtapa = @IdModeloEtapa";
    public static string InserirItem => $"INSERT INTO {TI} ({CI}) VALUES (@Id, @IdModeloEtapa, @Nome, @Descricao, @Obrigatorio, @ExigeAnexo, @Ordem, @DataCadastro, @DataAlteracao, @CriadoPor, @AlteradoPor)";
    public static string AtualizarItem => $"UPDATE {TI} SET Nome = @Nome, Descricao = @Descricao, Obrigatorio = @Obrigatorio, ExigeAnexo = @ExigeAnexo, Ordem = @Ordem, DataAlteracao = @DataAlteracao, AlteradoPor = @AlteradoPor WHERE Id = @Id";
    public static string ExcluirItem => $"DELETE FROM {TI} WHERE Id = @Id";
}
