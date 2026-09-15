-- Migration: Criar tabela Parceria e vincular em Receita/Despesa
-- Idempotente: pode ser re-executado com segurança

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Parceria')
BEGIN
    CREATE TABLE Parceria (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        IdParceiro UNIQUEIDENTIFIER NOT NULL,
        IdCliente UNIQUEIDENTIFIER NOT NULL,
        Valor DECIMAL(18,2) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_Parceria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
        CONSTRAINT FK_Parceria_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id),
        CONSTRAINT FK_Parceria_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
    );
    CREATE INDEX IX_Parceria_Usuario ON Parceria(IdUsuario);
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdParceria')
    ALTER TABLE Receita ADD IdParceria UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_Parceria')
    ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receita_Parceria' AND object_id = OBJECT_ID('Receita'))
    CREATE INDEX IX_Receita_Parceria ON Receita(IdParceria);

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Despesa') AND name = 'IdParceria')
    ALTER TABLE Despesa ADD IdParceria UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Despesa_Parceria')
    ALTER TABLE Despesa ADD CONSTRAINT FK_Despesa_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Despesa_Parceria' AND object_id = OBJECT_ID('Despesa'))
    CREATE INDEX IX_Despesa_Parceria ON Despesa(IdParceria);
