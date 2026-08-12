namespace PortalFinanceiro.Infrastructure.Sql;

internal static class UsuarioSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}Usuario";
    static string C => "Id, Nome, Email, SenhaHash, IsAdmin, Ativo, DataCadastro, DataAlteracao";
    public static string ObterPorId => $"SELECT {C} FROM {T} WHERE Id = @Id";
    public static string ObterPorEmail => $"SELECT {C} FROM {T} WHERE Email = @Email";
    public static string Listar => $"SELECT {C} FROM {T} ORDER BY Nome";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @Nome, @Email, @SenhaHash, @IsAdmin, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar => $"UPDATE {T} SET Nome = @Nome, Email = @Email, SenhaHash = @SenhaHash, IsAdmin = @IsAdmin, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
}
