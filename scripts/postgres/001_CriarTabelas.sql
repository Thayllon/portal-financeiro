-- Portal Financeiro - Schema unificado (PostgreSQL).
-- ATENÇÃO: executar somente em banco NOVO (from scratch).
-- Conjunto final consolidado em 2 scripts: 001_CriarTabelas.sql + 099_SeedBase.sql.
-- Bancos já criados evoluem apenas via scripts incrementais (quando existirem).

CREATE TABLE Usuario (
    Id UUID PRIMARY KEY,
    Nome VARCHAR(200) NOT NULL,
    Email VARCHAR(200) NOT NULL,
    SenhaHash VARCHAR(500) NOT NULL,
    IsAdmin BOOLEAN NOT NULL DEFAULT FALSE,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    PrimeiroAcesso BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX IX_Usuario_Email ON Usuario(Email) WHERE Ativo = TRUE;

CREATE TABLE ContaBancaria (
      Id UUID PRIMARY KEY,
      IdUsuario UUID NOT NULL,
      Nome VARCHAR(200) NOT NULL,
      Banco VARCHAR(100) NOT NULL,
      Tipo INT NOT NULL,
      EhPadrao BOOLEAN NOT NULL DEFAULT FALSE,
      Ativo BOOLEAN NOT NULL DEFAULT TRUE,
      DataCadastro TIMESTAMP NOT NULL,
      DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_ContaBancaria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
  );

CREATE TABLE Pessoa (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Telefone VARCHAR(30) NULL,
    Tipo INT NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Pessoa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE TABLE Parceria (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    IdParceiro UUID NOT NULL,
    IdCliente UUID NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    PercentualParceiro NUMERIC(5,2) NOT NULL DEFAULT 0,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Parceria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Parceria_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id),
    CONSTRAINT FK_Parceria_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
);

CREATE INDEX IX_Parceria_Usuario ON Parceria(IdUsuario);

CREATE TABLE Contrato (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    IdCliente UUID NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    EhRecorrente BOOLEAN NOT NULL DEFAULT FALSE,
    IdRegra UUID NULL,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Contrato_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Contrato_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id),
    CONSTRAINT FK_Contrato_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id)
);

CREATE INDEX IX_Contrato_Usuario ON Contrato(IdUsuario);

CREATE TABLE PermissaoUsuario (
    Id UUID PRIMARY KEY,
    UsuarioId UUID NOT NULL,
    Modulo VARCHAR(50) NOT NULL,
    Nivel INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_PermissaoUsuario_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id),
    CONSTRAINT UQ_PermissaoUsuario_UsuarioModulo UNIQUE (UsuarioId, Modulo)
);

CREATE TABLE CategoriaReceita (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    CategoriaPaiId UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_CategoriaReceita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaReceita_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaReceita(Id)
);

CREATE TABLE CategoriaDespesa (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    CategoriaPaiId UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_CategoriaDespesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaDespesa_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaDespesa(Id)
);

CREATE TABLE CategoriaServico (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    CategoriaPaiId UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_CategoriaServico_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_CategoriaServico_Pai FOREIGN KEY (CategoriaPaiId) REFERENCES CategoriaServico(Id)
);

CREATE TABLE RegraReceita (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Descricao VARCHAR(200) NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Dia INT NOT NULL,
    DiaUtil BOOLEAN NOT NULL DEFAULT FALSE,
    IdCategoria UUID NOT NULL,
    IdConta UUID NOT NULL,
    DataInicio TIMESTAMP NOT NULL,
    DataFim TIMESTAMP NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_RegraReceita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_RegraReceita_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_RegraReceita_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE RegraDespesa (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Descricao VARCHAR(200) NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Dia INT NOT NULL,
    DiaUtil BOOLEAN NOT NULL DEFAULT FALSE,
    IdCategoria UUID NOT NULL,
    IdConta UUID NOT NULL,
    DataInicio TIMESTAMP NOT NULL,
    DataFim TIMESTAMP NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_RegraDespesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_RegraDespesa_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_RegraDespesa_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id)
);

CREATE TABLE Receita (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Descricao VARCHAR(200) NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Data TIMESTAMP NOT NULL,
    IdConta UUID NOT NULL,
    IdCategoria UUID NOT NULL,
    IdSubcategoria UUID NULL,
    IdParceiro UUID NULL,
    IdCliente UUID NULL,
    IdParceria UUID NULL,
    IdContrato UUID NULL,
    Status INT NOT NULL DEFAULT 1,
    DataRealizacao TIMESTAMP NULL,
    IdRegra UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Receita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Receita_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_Receita_Subcategoria FOREIGN KEY (IdSubcategoria) REFERENCES CategoriaReceita(Id),
    CONSTRAINT FK_Receita_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Receita_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id),
    CONSTRAINT FK_Receita_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id),
    CONSTRAINT FK_Receita_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id),
    CONSTRAINT FK_Receita_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id),
    CONSTRAINT FK_Receita_Contrato FOREIGN KEY (IdContrato) REFERENCES Contrato(Id)
);

CREATE INDEX IX_Receita_Parceria ON Receita(IdParceria);
CREATE INDEX IX_Receita_Contrato ON Receita(IdContrato);

CREATE TABLE ReceitaServico (
    Id              UUID PRIMARY KEY,
    ReceitaId       UUID NOT NULL,
    CategoriaServicoId UUID NOT NULL,
    SubcategoriaServicoId UUID NULL,
    CONSTRAINT FK_ReceitaServico_Receita FOREIGN KEY (ReceitaId) REFERENCES Receita(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReceitaServico_CategoriaServico FOREIGN KEY (CategoriaServicoId) REFERENCES CategoriaServico(Id),
    CONSTRAINT FK_ReceitaServico_SubcategoriaServico FOREIGN KEY (SubcategoriaServicoId) REFERENCES CategoriaServico(Id)
);

CREATE INDEX IX_ReceitaServico_ReceitaId ON ReceitaServico(ReceitaId);

CREATE TABLE Despesa (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Descricao VARCHAR(200) NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Data TIMESTAMP NOT NULL,
    IdConta UUID NOT NULL,
    IdCategoria UUID NOT NULL,
    IdSubcategoria UUID NULL,
    Status INT NOT NULL DEFAULT 1,
    DataRealizacao TIMESTAMP NULL,
    IdRegra UUID NULL,
    IdReceitaOrigem UUID NULL,
    IdParceria UUID NULL,
    IdCliente UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Despesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Despesa_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_Despesa_Subcategoria FOREIGN KEY (IdSubcategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_Despesa_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Despesa_Regra FOREIGN KEY (IdRegra) REFERENCES RegraDespesa(Id),
    CONSTRAINT FK_Despesa_ReceitaOrigem FOREIGN KEY (IdReceitaOrigem) REFERENCES Receita(Id),
    CONSTRAINT FK_Despesa_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id),
    CONSTRAINT FK_Despesa_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
);

CREATE TABLE DespesaServico (
    Id              UUID PRIMARY KEY,
    DespesaId       UUID NOT NULL,
    CategoriaServicoId UUID NOT NULL,
    SubcategoriaServicoId UUID NULL,
    CONSTRAINT FK_DespesaServico_Despesa FOREIGN KEY (DespesaId) REFERENCES Despesa(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DespesaServico_CategoriaServico FOREIGN KEY (CategoriaServicoId) REFERENCES CategoriaServico(Id),
    CONSTRAINT FK_DespesaServico_SubcategoriaServico FOREIGN KEY (SubcategoriaServicoId) REFERENCES CategoriaServico(Id)
);

CREATE INDEX IX_DespesaServico_DespesaId ON DespesaServico(DespesaId);

CREATE INDEX IX_Despesa_Parceria ON Despesa(IdParceria);

CREATE TABLE CategoriaHistorico (
    Id UUID PRIMARY KEY,
    IdCategoria UUID NOT NULL,
    TipoCategoria INT NOT NULL,
    IdUsuario UUID NOT NULL,
    Acao INT NOT NULL,
    NomeAntigo VARCHAR(100) NULL,
    NomeNovo VARCHAR(100) NULL,
    CategoriaPaiIdAntiga UUID NULL,
    CategoriaPaiIdNova UUID NULL,
    DataCadastro TIMESTAMP NOT NULL,
    CONSTRAINT FK_CategoriaHistorico_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE INDEX IX_CategoriaHistorico_Categoria ON CategoriaHistorico(IdCategoria, TipoCategoria);