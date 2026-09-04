-- Migração: Cria tabela CategoriaServico (SQL Server)
-- Requer: 001_CriarTabelas.sql

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoriaServico')
BEGIN
    CREATE TABLE CategoriaServico (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Nome NVARCHAR(100) NOT NULL,
        CategoriaPaiId UNIQUEIDENTIFIER NULL,
        Ativo BIT NOT NULL DEFAULT 1,
        DataCadastro DATETIME2 NOT NULL,
        DataAlteracao DATETIME2 NOT NULL,
        CONSTRAINT FK_CategoriaServico_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
        CONSTRAINT FK_CategoriaServico_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaServico(Id)
    );
END
