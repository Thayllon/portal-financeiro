-- Migração 007: criar tabela Pessoa (clientes/parceiros)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('Pessoa'))
BEGIN
    CREATE TABLE Pessoa (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(150) NOT NULL,
        Telefone NVARCHAR(30) NULL,
        Tipo INT NOT NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_Pessoa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
    );
END;