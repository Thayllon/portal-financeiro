namespace PortalFinanceiro.Infrastructure.Sql;

internal static class UsuarioSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Usuario";
    static string C => "Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterPorEmail => $"SELECT {C} FROM {T} WHERE Email = @Email";
    public static string Listar => $"SELECT {C} FROM {T} ORDER BY Nome";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @Nome, @Email, @SenhaHash, @IsAdmin, @Ativo, @PrimeiroAcesso, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Email = @Email, SenhaHash = @SenhaHash, IsAdmin = @IsAdmin, Ativo = @Ativo, PrimeiroAcesso = @PrimeiroAcesso, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string Excluir => $"DELETE FROM {T} WHERE Id = @Id";
    public static string ContarVinculos
        => $"SELECT "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}ContaBancaria WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Pessoa WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Parceria WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Contrato WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}CategoriaReceita WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}CategoriaDespesa WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}CategoriaServico WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}RegraReceita WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}RegraDespesa WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Receita WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Despesa WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}CategoriaHistorico WHERE IdUsuario = @Id)";
}
