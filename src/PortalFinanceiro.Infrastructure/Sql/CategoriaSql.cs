namespace PortalFinanceiro.Infrastructure.Sql;

internal static class CategoriaSql
{
    public static string ObterPorId(string tabela) => $"SELECT Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao FROM {SqlDialect.Current.SchemaPrefix}{tabela} WHERE Id = @Id";
    public static string ListarAtivas(string tabela) => $"SELECT Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao FROM {SqlDialect.Current.SchemaPrefix}{tabela} WHERE Ativo = {SqlDialect.Current.BooleanTrue} ORDER BY Nome";
    public static string ListarPorPai(string tabela) => $"SELECT Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao FROM {SqlDialect.Current.SchemaPrefix}{tabela} WHERE CategoriaPaiId = @CategoriaPaiId AND Ativo = {SqlDialect.Current.BooleanTrue}";
    public static string Inserir(string tabela) => $"INSERT INTO {SqlDialect.Current.SchemaPrefix}{tabela} (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (@Id, @IdUsuario, @Nome, @CategoriaPaiId, @Ativo, @DataCadastro, @DataAlteracao)";
    public static string Atualizar(string tabela) => $"UPDATE {SqlDialect.Current.SchemaPrefix}{tabela} SET Nome = @Nome, CategoriaPaiId = @CategoriaPaiId, Ativo = @Ativo, DataAlteracao = @DataAlteracao WHERE Id = @Id";
    public static string Excluir(string tabela) => $"UPDATE {SqlDialect.Current.SchemaPrefix}{tabela} SET Ativo = {SqlDialect.Current.BooleanFalse}, DataAlteracao = {SqlDialect.Current.UtcTimestamp} WHERE Id = @Id";
}
