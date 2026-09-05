namespace PortalFinanceiro.Infrastructure.Sql;

internal static class PermissaoUsuarioSql
{
    static string T => $"{SqlDialect.Current.SchemaPrefix}PermissaoUsuario";
    static string C => "Id, UsuarioId, Modulo, Nivel";
    public static string ObterPorUsuarioId => $"SELECT {C} FROM {T} WHERE UsuarioId = @UsuarioId";
    public static string ObterPorUsuarioEModulo => $"SELECT {C} FROM {T} WHERE UsuarioId = @UsuarioId AND Modulo = @Modulo";
    public static string Inserir => $"INSERT INTO {T} ({C}) VALUES (@Id, @UsuarioId, @Modulo, @Nivel)";
    public static string Atualizar => $"UPDATE {T} SET Nivel = @Nivel WHERE Id = @Id";
    public static string Excluir => $"DELETE FROM {T} WHERE Id = @Id";
    public static string ExcluirPorUsuarioId => $"DELETE FROM {T} WHERE UsuarioId = @UsuarioId";
}
