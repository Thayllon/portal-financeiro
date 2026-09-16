namespace PortalFinanceiro.Infrastructure.Sql;

internal static class DespesaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Despesa";
    static string C => "Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, IdRegra, IdReceitaOrigem, IdParceria, IdCliente, Ativo, DataCadastro, DataAlteracao";
    static string Extras => $"{T}.IdReceitaOrigem, {T}.IdParceria, {T}.IdCliente";
    static string JoinsExtras => string.Empty;
    static string SetAtualizar => "Descricao = @Descricao, Valor = @Valor, Data = @Data, IdConta = @IdConta, IdCategoria = @IdCategoria, IdSubcategoria = @IdSubcategoria, Status = @Status, DataRealizacao = @DataRealizacao, Ativo = @Ativo, DataAlteracao = @DataAlteracao, IdParceria = @IdParceria, IdCliente = @IdCliente";

    public static string ObterPorId => LancamentoSql.ObterPorId(T, "CategoriaDespesa", Extras, JoinsExtras);
    public static string ListarPorMes => LancamentoSql.ListarPorMes(T, "CategoriaDespesa", Extras, JoinsExtras);
    public static string ContarPorCategoria => LancamentoSql.ContarPorCategoria(T);
    public static string ListarPorParceria => LancamentoSql.ListarPorParceria(T, "CategoriaDespesa", Extras, JoinsExtras);
    public static string ContarPorSubcategoria => LancamentoSql.ContarPorSubcategoria(T);
    public static string ContarPorRegra => LancamentoSql.ContarPorRegra(T);
    public static string ListarPorRegra => LancamentoSql.ListarPorRegra(T, "CategoriaDespesa", Extras, JoinsExtras);
    public static string ListarPorReceitaOrigem => LancamentoSql.ListarPorReceitaOrigem(T, "CategoriaDespesa", Extras, JoinsExtras);
    public static string Inserir => LancamentoSql.Inserir(T, C);
    public static string Atualizar => LancamentoSql.Atualizar(T, SetAtualizar);
    public static string Excluir => LancamentoSql.Excluir(T);
    public static string ResumoAnualPorMes => LancamentoSql.ResumoAnualPorMes(T);
    public static string ResumoAnualPorConta => LancamentoSql.ResumoAnualPorConta(T);
    public static string ResumoAnualPorCategoria => LancamentoSql.ResumoAnualPorCategoria(T, "CategoriaDespesa");
}