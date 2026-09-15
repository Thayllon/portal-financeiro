namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ReceitaSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Receita";
    static string C => "Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, IdParceiro, IdCliente, IdParceria, Status, DataRealizacao, IdRegra, Ativo, DataCadastro, DataAlteracao";
    static string Extras => $"{T}.IdParceiro, {T}.IdCliente, {T}.IdParceria, par.Nome AS Parceiro";
    static string JoinsExtras => $"LEFT JOIN {SqlDialect.Current.SchemaPrefix}Pessoa par ON {T}.IdParceiro = par.Id";
    static string SetAtualizar => "Descricao = @Descricao, Valor = @Valor, Data = @Data, IdConta = @IdConta, IdCategoria = @IdCategoria, IdSubcategoria = @IdSubcategoria, IdParceiro = @IdParceiro, IdCliente = @IdCliente, IdParceria = @IdParceria, Status = @Status, DataRealizacao = @DataRealizacao, Ativo = @Ativo, DataAlteracao = @DataAlteracao";

    public static string ObterPorId => LancamentoSql.ObterPorId(T, "CategoriaReceita", Extras, JoinsExtras);
    public static string ListarPorMes => LancamentoSql.ListarPorMes(T, "CategoriaReceita", Extras, JoinsExtras);
    public static string ContarPorCategoria => LancamentoSql.ContarPorCategoria(T);
    public static string ListarPorParceria => LancamentoSql.ListarPorParceria(T, "CategoriaReceita", Extras, JoinsExtras);
    public static string ContarPorSubcategoria => LancamentoSql.ContarPorSubcategoria(T);
    public static string ContarPorRegra => LancamentoSql.ContarPorRegra(T);
    public static string ListarPorRegra => LancamentoSql.ListarPorRegra(T, "CategoriaReceita", Extras, JoinsExtras);
    public static string Inserir => LancamentoSql.Inserir(T, C);
    public static string Atualizar => LancamentoSql.Atualizar(T, SetAtualizar);
    public static string Excluir => LancamentoSql.Excluir(T);
    public static string ResumoAnualPorMes => LancamentoSql.ResumoAnualPorMes(T);
    public static string ResumoAnualPorConta => LancamentoSql.ResumoAnualPorConta(T);
}