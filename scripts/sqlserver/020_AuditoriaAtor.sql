-- Portal Financeiro - Auditoria de ator (SQL Server, banco local).
-- 020: registra quem criou e quem alterou por último (CriadoPor / AlteradoPor).
-- Nuláveis: dados anteriores à auditoria permanecem NULL (gravação daqui para frente).
-- Sem FK para Usuario de propósito: o histórico sobrevive mesmo se o usuário for excluído.

DECLARE @Tabelas TABLE (Nome SYSNAME);
INSERT INTO @Tabelas (Nome) VALUES
    (N'ContaBancaria'), (N'Pessoa'), (N'Parceria'), (N'Contrato'),
    (N'CategoriaReceita'), (N'CategoriaDespesa'), (N'CategoriaServico'),
    (N'RegraReceita'), (N'RegraDespesa'), (N'Receita'), (N'Despesa'),
    (N'Processo'), (N'ProcessoEtapa');

DECLARE @Tabela SYSNAME;
DECLARE @Sql NVARCHAR(MAX);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT Nome FROM @Tabelas;
OPEN cur;
FETCH NEXT FROM cur INTO @Tabela;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF COL_LENGTH(@Tabela, 'CriadoPor') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@Tabela) + N' ADD CriadoPor UNIQUEIDENTIFIER NULL';
        EXEC(@Sql);
    END;
    IF COL_LENGTH(@Tabela, 'AlteradoPor') IS NULL
    BEGIN
        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@Tabela) + N' ADD AlteradoPor UNIQUEIDENTIFIER NULL';
        EXEC(@Sql);
    END;
    FETCH NEXT FROM cur INTO @Tabela;
END;
CLOSE cur;
DEALLOCATE cur;
