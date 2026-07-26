namespace PortalFinanceiro.Infrastructure.Sql;

internal static class DespesaMensalSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}DespesaMensal";
    static string C => "Id, IdDespesaRecorrente, Mes, Ano, Valor, DataPagamento, Status, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorMes => $@"
        SELECT d.* FROM {T} d
        INNER JOIN {SqlDialect.Current.SchemaPrefix}DespesaRecorrente dr ON d.IdDespesaRecorrente = dr.Id
        WHERE dr.IdUsuario = @IdUsuario AND d.Mes = @Mes AND d.Ano = @Ano AND d.Ativo = 1
        ORDER BY d.Status, dr.Descricao";
    public static string ListarPorDespesaRecorrente => $"SELECT {C} FROM {T} WHERE IdDespesaRecorrente = @IdDespesaRecorrente AND Ativo = 1 ORDER BY Ano, Mes";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdDespesaRecorrente, @Mes, @Ano, @Valor, @DataPagamento, @Status, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Valor = @Valor, DataPagamento = @DataPagamento, Status = @Status, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
