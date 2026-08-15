-- Migração 006: adicionar coluna IdReceitaOrigem na tabela Despesa
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Despesa') AND name = 'IdReceitaOrigem')
BEGIN
    ALTER TABLE Despesa ADD IdReceitaOrigem UNIQUEIDENTIFIER NULL;
END;
