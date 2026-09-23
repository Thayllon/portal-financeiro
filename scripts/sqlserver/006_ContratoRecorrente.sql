-- Contrato recorrente: EhRecorrente + IdRegra + geração de receitas.
-- Idempotente para bancos já criados (001 já contém colunas no from-scratch).

IF COL_LENGTH('Contrato', 'EhRecorrente') IS NULL
    ALTER TABLE Contrato ADD EhRecorrente BIT NOT NULL CONSTRAINT DF_Contrato_EhRecorrente DEFAULT 0;

IF COL_LENGTH('Contrato', 'IdRegra') IS NULL
    ALTER TABLE Contrato ADD IdRegra UNIQUEIDENTIFIER NULL;

IF OBJECT_ID('FK_Contrato_Regra', 'F') IS NULL
    ALTER TABLE Contrato ADD CONSTRAINT FK_Contrato_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id);
