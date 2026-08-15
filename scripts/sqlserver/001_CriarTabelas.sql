-- Portal Financeiro - Schema unificado (SQL Server).
-- ATENÇÃO: executar somente em banco NOVO (from scratch). O banco de desenvolvimento
-- já existente foi migrado incrementalmente e NÃO deve receber este script.
-- Conjunto final consolidado em 2 scripts: 001_CriarTabelas.sql + 099_SeedBase.sql.

CREATE TABLE Usuario (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Nome NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    SenhaHash NVARCHAR(500) NOT NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL
);

CREATE UNIQUE INDEX IX_Usuario_Email ON Usuario(Email) WHERE Ativo = 1;

CREATE TABLE ContaBancaria (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(200) NOT NULL,
    Banco NVARCHAR(100) NOT NULL,
    Tipo INT NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_ContaBancaria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE TABLE CategoriaReceita (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    CategoriaPaiId UNIQUEIDENTIFIER NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_CategoriaReceita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaReceita_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaReceita(Id)
);

CREATE TABLE CategoriaDespesa (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    CategoriaPaiId UNIQUEIDENTIFIER NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_CategoriaDespesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaDespesa_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaDespesa(Id)
);

CREATE TABLE RegraReceita (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Dia INT NOT NULL,
    DiaUtil BIT NOT NULL DEFAULT 0,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataFim DATETIME2 NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_RegraReceita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_RegraReceita_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_RegraReceita_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE RegraDespesa (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Dia INT NOT NULL,
    DiaUtil BIT NOT NULL DEFAULT 0,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataFim DATETIME2 NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_RegraDespesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_RegraDespesa_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_RegraDespesa_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE Receita (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Data DATETIME2 NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdSubcategoria UNIQUEIDENTIFIER NULL,
    Status INT NOT NULL DEFAULT 1,
    DataRealizacao DATETIME2 NULL,
    IdRegra UNIQUEIDENTIFIER NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_Receita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Receita_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_Receita_Subcategoria FOREIGN KEY (IdSubcategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_Receita_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Receita_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id)
);

CREATE TABLE Despesa (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Data DATETIME2 NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdSubcategoria UNIQUEIDENTIFIER NULL,
    Status INT NOT NULL DEFAULT 1,
    DataRealizacao DATETIME2 NULL,
    IdRegra UNIQUEIDENTIFIER NULL,
    IdReceitaOrigem UNIQUEIDENTIFIER NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_Despesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Despesa_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_Despesa_Subcategoria FOREIGN KEY (IdSubcategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_Despesa_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Despesa_Regra FOREIGN KEY (IdRegra) REFERENCES RegraDespesa(Id),
    CONSTRAINT FK_Despesa_ReceitaOrigem FOREIGN KEY (IdReceitaOrigem) REFERENCES Receita(Id)
);

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