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
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}CategoriaHistorico WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}ModeloProcesso WHERE IdUsuario = @Id) + "
        + $"(SELECT COUNT(*) FROM {SqlDialect.Current.SchemaPrefix}Processo WHERE IdUsuario = @Id)";

    public static IEnumerable<string> ExcluirEmCascata()
    {
        var p = SqlDialect.Current.SchemaPrefix;
        yield return $"DELETE FROM {p}ReceitaServico WHERE ReceitaId IN (SELECT Id FROM {p}Receita WHERE IdUsuario = @Id)";
        yield return $"DELETE FROM {p}DespesaServico WHERE DespesaId IN (SELECT Id FROM {p}Despesa WHERE IdUsuario = @Id)";
        yield return $"DELETE FROM {p}ProcessoAnexo WHERE IdProcessoEtapaItem IN (SELECT Id FROM {p}ProcessoEtapaItem WHERE IdProcessoEtapa IN (SELECT Id FROM {p}ProcessoEtapa WHERE IdProcesso IN (SELECT Id FROM {p}Processo WHERE IdUsuario = @Id)))";
        yield return $"DELETE FROM {p}ProcessoEtapaItem WHERE IdProcessoEtapa IN (SELECT Id FROM {p}ProcessoEtapa WHERE IdProcesso IN (SELECT Id FROM {p}Processo WHERE IdUsuario = @Id))";
        yield return $"DELETE FROM {p}ProcessoEtapa WHERE IdProcesso IN (SELECT Id FROM {p}Processo WHERE IdUsuario = @Id)";
        yield return $"DELETE FROM {p}Receita WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}Despesa WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}Processo WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}ModeloItem WHERE IdModeloEtapa IN (SELECT Id FROM {p}ModeloEtapa WHERE IdModeloProcesso IN (SELECT Id FROM {p}ModeloProcesso WHERE IdUsuario = @Id))";
        yield return $"DELETE FROM {p}ModeloEtapa WHERE IdModeloProcesso IN (SELECT Id FROM {p}ModeloProcesso WHERE IdUsuario = @Id)";
        yield return $"DELETE FROM {p}ModeloProcesso WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}Contrato WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}Parceria WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}RegraReceita WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}RegraDespesa WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}ContaBancaria WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}Pessoa WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}CategoriaReceita WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}CategoriaDespesa WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}CategoriaServico WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}CategoriaHistorico WHERE IdUsuario = @Id";
        yield return $"DELETE FROM {p}PermissaoUsuario WHERE UsuarioId = @Id";
        yield return $"DELETE FROM {p}Usuario WHERE Id = @Id";
    }
}
