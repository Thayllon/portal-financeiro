namespace PortalFinanceiro.Infrastructure.Sql;

internal static class ProLaboreSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}ProLabore";
    static string C => "Id, IdUsuario, Ano, Mes, Valor, PercentualInss, IdConta, Ativo, DataCadastro, DataAlteracao";
    static string CComNomes => $@"{T}.Id, {T}.IdUsuario, {T}.Ano, {T}.Mes, {T}.Valor, {T}.PercentualInss, {T}.IdConta, {T}.Ativo, {T}.DataCadastro, {T}.DataAlteracao,
        cb.Nome AS Conta";
    static string TJoin => $"{T} LEFT JOIN {SqlDialect.Current.SchemaPrefix}ContaBancaria cb ON {T}.IdConta = cb.Id";
    public static string ObterPorId => $"SELECT {CComNomes} FROM {TJoin} WHERE {T}.Id = @Id";
    public static string ObterPorMes => $"SELECT {CComNomes} FROM {TJoin} WHERE {T}.IdUsuario = @IdUsuario AND {T}.Mes = @Mes AND {T}.Ano = @Ano AND {T}.Ativo = 1";
    public static string ListarPorUsuario => $"SELECT {CComNomes} FROM {TJoin} WHERE {T}.IdUsuario = @IdUsuario AND {T}.Ativo = 1 ORDER BY {T}.Ano DESC, {T}.Mes DESC";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @IdUsuario, @Ano, @Mes, @Valor, @PercentualInss, @IdConta, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Valor = @Valor, PercentualInss = @PercentualInss, IdConta = @IdConta, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string Excluir => $"UPDATE {T} SET Ativo = 0, DataAlteracao = GETUTCDATE() WHERE Id = @Id";
}