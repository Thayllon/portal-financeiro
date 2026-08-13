-- Migração 200: remoção do módulo Pró-labore / encargo INSS.
-- Idempotente: executa apenas o que ainda existe (bancos que já rodaram 001/099).

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Despesa_ProLaboreOrigem')
BEGIN
    ALTER TABLE Despesa DROP CONSTRAINT FK_Despesa_ProLaboreOrigem;
END;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Despesa') AND name = 'IdProLaboreOrigem')
BEGIN
    ALTER TABLE Despesa DROP COLUMN IdProLaboreOrigem;
END;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProLabore')
BEGIN
    DROP TABLE ProLabore;
END;
