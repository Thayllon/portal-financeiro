namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ReceitaMensalSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ReceitaMensal";
    static string C => "Id, IdReceitaRecorrente, Mes, Ano, Valor, DataRecebimento, Status, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ListarPorMes => $@"
        SELECT r.* FROM {T} r
        INNER JOIN {SqlDialect.Current.SchemaPrefix}ReceitaRecorrente rr ON r.IdReceitaRecorrente = rr.Id
        WHERE rr.IdUsuario = @IdUsuario AND r.Mes = @Mes AND r.Ano = @Ano AND r.Ativo = 1
        ORDER BY r.Status, rr.Descricao";
    public static string ListarPorReceitaRecorrente => $"SELECT {C} FROM {T} WHERE IdReceitaRecorrente = @IdReceitaRecorrente AND Ativo = 1 ORDER BY Ano, Mes";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdReceitaRecorrente, @Mes, @Ano, @Valor, @DataRecebimento, @Status, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Valor = @Valor, DataRecebimento = @DataRecebimento, Status = @Status, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
