-- Contratos: tabela + vínculo em Receita + módulo de permissão.
-- Idempotente para bancos já criados antes desta migração (001 já contém o schema final).
IF OBJECT_ID('Contrato', 'U') IS NULL
BEGIN
    CREATE TABLE Contrato (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        IdCliente UNIQUEIDENTIFIER NOT NULL,
        Valor DECIMAL(18,2) NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_Contrato_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
        CONSTRAINT FK_Contrato_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
    );
    CREATE INDEX IX_Contrato_Usuario ON Contrato(IdUsuario);
END

IF COL_LENGTH('Receita', 'IdContrato') IS NULL
    ALTER TABLE Receita ADD IdContrato UNIQUEIDENTIFIER NULL CONSTRAINT FK_Receita_Contrato FOREIGN KEY (IdContrato) REFERENCES Contrato(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receita_Contrato')
    CREATE INDEX IX_Receita_Contrato ON Receita(IdContrato);

-- Garante o módulo 'contratos' para usuários que ainda não o possuem.
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), Id, 'contratos', 0
FROM Usuario
WHERE Id NOT IN (
    SELECT UsuarioId FROM PermissaoUsuario WHERE Modulo = 'contratos'
);
