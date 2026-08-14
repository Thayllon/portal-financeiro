-- Migração 005: criar tabela CategoriaHistorico (não foi criada pelo 001 antigo)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CategoriaHistorico')
BEGIN
    CREATE TABLE CategoriaHistorico (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        IdCategoria UNIQUEIDENTIFIER NOT NULL,
        TipoCategoria INT NOT NULL,
        IdUsuario UNIQUEIDENTIFIER NOT NULL,
        Acao INT NOT NULL,
        NomeAntigo NVARCHAR(100) NULL,
        NomeNovo NVARCHAR(100) NULL,
        CategoriaPaiIdAntiga UNIQUEIDENTIFIER NULL,
        CategoriaPaiIdNova UNIQUEIDENTIFIER NULL,
        DataCadastro DATETIME2 NOT NULL,
        CONSTRAINT FK_CategoriaHistorico_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
    );

    CREATE INDEX IX_CategoriaHistorico_Categoria ON CategoriaHistorico(IdCategoria, TipoCategoria);
END;
