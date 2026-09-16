namespace PortalFinanceiro.Infrastructure.Sql;

internal static class LancamentoSql
{
    static string S => SqlDialect.Current.SchemaPrefix;

    static string ColunasNomes(string t, string tabelaCategoria, string extras)
        => $@"{t}.Id, {t}.IdUsuario, {t}.Descricao, {t}.Valor, {t}.Data, {t}.IdConta, {t}.IdCategoria, {t}.IdSubcategoria,
        {t}.Status, {t}.DataRealizacao, {t}.IdRegra, {t}.Ativo, {t}.DataCadastro, {t}.DataAlteracao,
        {extras},
        cb.Nome AS Conta,
        cat.Nome AS Categoria,
        sub.Nome AS Subcategoria,
        cli.Nome AS Cliente,
        pa.Valor AS ParceriaValor,
        CASE WHEN pa.Id IS NULL THEN '' ELSE CONCAT(COALESCE(par2.Nome,''), ' - ', COALESCE(cli2.Nome,'')) END AS Parceria";

    static string Joins(string t, string tabelaCategoria, string joinsExtras)
        => $@"
        LEFT JOIN {S}ContaBancaria cb ON {t}.IdConta = cb.Id
        LEFT JOIN {S}{tabelaCategoria} cat ON {t}.IdCategoria = cat.Id
        LEFT JOIN {S}{tabelaCategoria} sub ON {t}.IdSubcategoria = sub.Id
        LEFT JOIN {S}Pessoa cli ON {t}.IdCliente = cli.Id
        LEFT JOIN {S}Parceria pa ON {t}.IdParceria = pa.Id
        LEFT JOIN {S}Pessoa par2 ON pa.IdParceiro = par2.Id
        LEFT JOIN {S}Pessoa cli2 ON pa.IdCliente = cli2.Id
        {joinsExtras}";

    public static string ObterPorId(string t, string tabelaCategoria, string extras, string joinsExtras)
        => $"SELECT {ColunasNomes(t, tabelaCategoria, extras)} FROM {t} {Joins(t, tabelaCategoria, joinsExtras)} WHERE {t}.Id = @Id";

    public static string ListarPorMes(string t, string tabelaCategoria, string extras, string joinsExtras)
        => $@"
        SELECT {ColunasNomes(t, tabelaCategoria, extras)} FROM {t} {Joins(t, tabelaCategoria, joinsExtras)}
        WHERE {t}.IdUsuario = @IdUsuario AND {t}.Ativo = 1
          AND MONTH({t}.Data) = @Mes AND YEAR({t}.Data) = @Ano
          AND (@IdConta IS NULL OR {t}.IdConta = @IdConta)
          AND (@IdCategoria IS NULL OR {t}.IdCategoria = @IdCategoria)
          AND (@Status IS NULL OR {t}.Status = @Status)
          AND (@Busca IS NULL OR {t}.Descricao LIKE '%' + @Busca + '%')
        ORDER BY {t}.Status, {t}.Data";

    public static string ContarPorCategoria(string t)
        => $"SELECT COUNT(*) FROM {t} WHERE IdCategoria = @IdCategoria AND Ativo = 1";

    public static string ListarPorParceria(string t, string tabelaCategoria, string extras, string joinsExtras)
        => $"SELECT {ColunasNomes(t, tabelaCategoria, extras)} FROM {t} {Joins(t, tabelaCategoria, joinsExtras)} WHERE {t}.IdParceria = @IdParceria AND {t}.Ativo = 1 ORDER BY {t}.Data";

    public static string ContarPorSubcategoria(string t)
        => $"SELECT COUNT(*) FROM {t} WHERE IdSubcategoria = @IdSubcategoria AND Ativo = 1";

    public static string ContarPorRegra(string t)
        => $"SELECT COUNT(*) FROM {t} WHERE IdRegra = @IdRegra AND Ativo = 1";

    public static string ListarPorRegra(string t, string tabelaCategoria, string extras, string joinsExtras)
        => $"SELECT {ColunasNomes(t, tabelaCategoria, extras)} FROM {t} {Joins(t, tabelaCategoria, joinsExtras)} WHERE {t}.IdRegra = @IdRegra AND {t}.Ativo = 1 ORDER BY {t}.Data";

    public static string ListarPorReceitaOrigem(string t, string tabelaCategoria, string extras, string joinsExtras)
        => $"SELECT {ColunasNomes(t, tabelaCategoria, extras)} FROM {t} {Joins(t, tabelaCategoria, joinsExtras)} WHERE {t}.IdReceitaOrigem = @IdReceitaOrigem AND {t}.Ativo = 1";

    public static string Inserir(string t, string colunas)
    {
        var valores = string.Join(", ", colunas.Split(", ").Select(c => $"@{c}"));
        return $"INSERT INTO {t} ({colunas}) VALUES ({valores})";
    }

    public static string Atualizar(string t, string setColunas)
        => $"UPDATE {t} SET {setColunas} WHERE Id = @Id";

    public static string Excluir(string t)
        => $"UPDATE {t} SET Ativo = 0, DataAlteracao = GETUTCDATE() WHERE Id = @Id";

    public static string ResumoAnualPorMes(string t)
        => $@"
        SELECT MONTH({t}.Data) AS Mes,
               SUM({t}.Valor) AS Total,
               SUM(CASE WHEN {t}.Status = 2 THEN {t}.Valor ELSE 0 END) AS TotalRealizado
        FROM {t}
        WHERE {t}.IdUsuario = @IdUsuario AND {t}.Ativo = 1 AND YEAR({t}.Data) = @Ano
          AND (@IdConta IS NULL OR {t}.IdConta = @IdConta)
        GROUP BY MONTH({t}.Data)";

    public static string ResumoAnualPorConta(string t)
        => $@"
        SELECT cb.Nome AS NomeConta, cb.Banco, cb.Tipo,
               SUM({t}.Valor) AS Total,
               SUM(CASE WHEN {t}.Status = 2 THEN {t}.Valor ELSE 0 END) AS TotalRealizado
        FROM {t}
        LEFT JOIN {S}ContaBancaria cb ON {t}.IdConta = cb.Id
        WHERE {t}.IdUsuario = @IdUsuario AND {t}.Ativo = 1 AND YEAR({t}.Data) = @Ano
          AND (@IdConta IS NULL OR {t}.IdConta = @IdConta)
        GROUP BY cb.Nome, cb.Banco, cb.Tipo
        HAVING SUM({t}.Valor) > 0";

    public static string ResumoAnualPorCategoria(string t, string tabelaCategoria)
        => $@"
        SELECT COALESCE(cat.Nome, 'Sem categoria') AS Categoria,
               COALESCE(sub.Nome, '') AS Subcategoria,
               SUM({t}.Valor) AS Total
        FROM {t}
        LEFT JOIN {S}{tabelaCategoria} cat ON {t}.IdCategoria = cat.Id
        LEFT JOIN {S}{tabelaCategoria} sub ON {t}.IdSubcategoria = sub.Id
        WHERE {t}.IdUsuario = @IdUsuario AND {t}.Ativo = 1 AND YEAR({t}.Data) = @Ano
          AND (@IdConta IS NULL OR {t}.IdConta = @IdConta)
        GROUP BY cat.Nome, sub.Nome";
}