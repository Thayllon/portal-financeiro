-- Migration: Adicionar campos de parceiro, cliente e serviços à tabela Receita
-- Idempotente: pode ser re-executado com segurança

-- Colunas
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdParceiro')
    ALTER TABLE Receita ADD IdParceiro UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdCategoriaServico')
    ALTER TABLE Receita ADD IdCategoriaServico UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdSubcategoriaServico')
    ALTER TABLE Receita ADD IdSubcategoriaServico UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdCliente')
    ALTER TABLE Receita ADD IdCliente UNIQUEIDENTIFIER NULL;

-- Constraints
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_Parceiro')
    ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_CategoriaServico')
    ALTER TABLE Receita ADD CONSTRAINT FK_Receita_CategoriaServico FOREIGN KEY (IdCategoriaServico) REFERENCES CategoriaServico(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_SubcategoriaServico')
    ALTER TABLE Receita ADD CONSTRAINT FK_Receita_SubcategoriaServico FOREIGN KEY (IdSubcategoriaServico) REFERENCES CategoriaServico(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_Cliente')
    ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);
