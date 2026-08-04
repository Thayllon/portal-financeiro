namespace PortalFinanceiro.Infrastructure.Sql;

internal static class DespesaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Despesa";
    static string C => "Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, IdRegra, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Descricao, {T}.Valor, {T}.Data, {T}.IdConta, {T}.IdCategoria, {T}.IdSubcategoria, {T}.Status, {T}.DataRealizacao, {T}.IdRegra, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        cb.Nome AS Conta,
        cat.Nome AS Categoria,
        sub.Nome AS Subcategoria";
    static string Joins => $@"
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}ContaBancaria cb ON {T}.IdConta = cb.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}CategoriaDespesa cat ON {T}.IdCategoria = cat.Id
        LEFT JOIN {SqlDialect.Current.SchemaPrefix}CategoriaDespesa sub ON {T}.IdSubcategoria = sub.Id";
    public static string ObterPorId => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.Id = @Id";
    public static string ListarPorMes => $@"
        SELECT {CComNomes} FROM {T} {Joins}
        WHERE {T}.IdUsuario = @IdUsuario AND {T}.Ativo = 1
          AND MONTH({T}.Data) = @Mes AND YEAR({T}.Data) = @Ano
          AND (@IdConta IS NULL OR {T}.IdConta = @IdConta)
          AND (@IdCategoria IS NULL OR {T}.IdCategoria = @IdCategoria)
          AND (@Status IS NULL OR {T}.Status = @Status)
          AND (@Busca IS NULL OR {T}.Descricao LIKE '%' + @Busca + '%')
        ORDER BY {T}.Status, {T}.Data";
    public static string ContarPorCategoria => $"SELECT COUNT(*) FROM {T} WHERE IdCategoria = @IdCategoria AND Ativo = 1";
    public static string ContarPorSubcategoria => $"SELECT COUNT(*) FROM {T} WHERE IdSubcategoria = @IdSubcategoria AND Ativo = 1";
    public static string ContarPorRegra => $"SELECT COUNT(*) FROM {T} WHERE IdRegra = @IdRegra AND Ativo = 1";
    public static string ListarPorRegra => $"SELECT {CComNomes} FROM {T} {Joins} WHERE {T}.IdRegra = @IdRegra AND {T}.Ativo = 1 ORDER BY {T}.Data";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Descricao, @Valor, @Data, @IdConta, @IdCategoria, @IdSubcategoria, @Status, @DataRealizacao, @IdRegra, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Descricao = @Descricao, Valor = @Valor, Data = @Data, IdConta = @IdConta, IdCategoria = @IdCategoria, IdSubcategoria = @IdSubcategoria, Status = @Status, DataRealizacao = @DataRealizacao, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
