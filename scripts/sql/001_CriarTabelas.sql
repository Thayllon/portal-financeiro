CREATE TABLE Usuario (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Nome NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    SenhaHash NVARCHAR(500) NOT NULL,
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
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_CategoriaReceita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE TABLE CategoriaDespesa (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Nome NVARCHAR(100) NOT NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_CategoriaDespesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE TABLE ReceitaRecorrente (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Dia INT NOT NULL,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataFim DATETIME2 NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_ReceitaRecorrente_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_ReceitaRecorrente_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_ReceitaRecorrente_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE DespesaRecorrente (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdUsuario UNIQUEIDENTIFIER NOT NULL,
    Descricao NVARCHAR(200) NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    Dia INT NOT NULL,
    IdCategoria UNIQUEIDENTIFIER NOT NULL,
    IdConta UNIQUEIDENTIFIER NOT NULL,
    DataInicio DATETIME2 NOT NULL,
    DataFim DATETIME2 NULL,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_DespesaRecorrente_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_DespesaRecorrente_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_DespesaRecorrente_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE ReceitaMensal (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdReceitaRecorrente UNIQUEIDENTIFIER NOT NULL,
    Mes INT NOT NULL,
    Ano INT NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    DataRecebimento DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 1,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_ReceitaMensal_Recorrente FOREIGN KEY (IdReceitaRecorrente) REFERENCES ReceitaRecorrente(Id)
);

CREATE TABLE DespesaMensal (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    IdDespesaRecorrente UNIQUEIDENTIFIER NOT NULL,
    Mes INT NOT NULL,
    Ano INT NOT NULL,
    Valor DECIMAL(18,2) NOT NULL,
    DataPagamento DATETIME2 NULL,
    Status INT NOT NULL DEFAULT 1,
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_DespesaMensal_Recorrente FOREIGN KEY (IdDespesaRecorrente) REFERENCES DespesaRecorrente(Id)
);
