-- CategoriaReceita: suporte a subcategoria
ALTER TABLE CategoriaReceita ADD CategoriaPaiId UNIQUEIDENTIFIER NULL;
ALTER TABLE CategoriaDespesa ADD CategoriaPaiId UNIQUEIDENTIFIER NULL;

-- RegraRecorrente: comportamento "repete" para receitas
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

-- RegraRecorrente: comportamento "repete" para despesas
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

-- Receita unificada (avulsa + parcela de regra)
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
    CONSTRAINT FK_Receita_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Receita_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id)
);

-- Despesa unificada (avulsa + parcela de regra)
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
    Ativo BIT NOT NULL DEFAULT 1,
    DataCadastro DATETIME2 NOT NULL,
    DataAlteracao DATETIME2 NOT NULL,
    CONSTRAINT FK_Despesa_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Despesa_Categoria FOREIGN KEY (IdCategoria) REFERENCES CategoriaDespesa(Id),
    CONSTRAINT FK_Despesa_Conta FOREIGN KEY (IdConta) REFERENCES ContaBancaria(Id),
    CONSTRAINT FK_Despesa_Regra FOREIGN KEY (IdRegra) REFERENCES RegraDespesa(Id)
);

-- Migrar dados existentes (receitas recorrentes -> regras)
INSERT INTO RegraReceita (Id, IdUsuario, Descricao, Valor, Dia, DiaUtil, IdCategoria, IdConta, DataInicio, DataFim, Ativo, DataCadastro, DataAlteracao)
SELECT Id, IdUsuario, Descricao, Valor, Dia, DiaUtil, IdCategoria, IdConta, DataInicio, COALESCE(DataFim, DataInicio), Ativo, DataCadastro, DataAlteracao
FROM ReceitaRecorrente;

-- Migrar dados existentes (receitas mensais -> receitas)
INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, Status, DataRealizacao, IdRegra, Ativo, DataCadastro, DataAlteracao)
SELECT m.Id, r.IdUsuario, r.Descricao, m.Valor,
       DATEFROMPARTS(m.Ano, m.Mes, 1),
       r.IdConta, r.IdCategoria, m.Status, m.DataRecebimento, m.IdReceitaRecorrente,
       m.Ativo, m.DataCadastro, m.DataAlteracao
FROM ReceitaMensal m
INNER JOIN ReceitaRecorrente r ON m.IdReceitaRecorrente = r.Id;

-- Remover tabelas antigas
DROP TABLE ReceitaMensal;
DROP TABLE DespesaMensal;
DROP TABLE ReceitaRecorrente;
DROP TABLE DespesaRecorrente;
