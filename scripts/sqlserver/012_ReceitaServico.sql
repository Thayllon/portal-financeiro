-- Migration: Tabela de junção ReceitaServico (muitos-para-muitos)
-- Idempotente: pode ser re-executado com segurança

-- 1. Criar tabela de junção (se não existir)
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReceitaServico')
BEGIN
    CREATE TABLE ReceitaServico (
        Id              UNIQUEIDENTIFIER PRIMARY KEY,
        ReceitaId       UNIQUEIDENTIFIER NOT NULL,
        CategoriaServicoId UNIQUEIDENTIFIER NOT NULL,
        SubcategoriaServicoId UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_ReceitaServico_Receita FOREIGN KEY (ReceitaId) REFERENCES Receita(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ReceitaServico_CategoriaServico FOREIGN KEY (CategoriaServicoId) REFERENCES CategoriaServico(Id),
        CONSTRAINT FK_ReceitaServico_SubcategoriaServico FOREIGN KEY (SubcategoriaServicoId) REFERENCES CategoriaServico(Id)
    );

    CREATE INDEX IX_ReceitaServico_ReceitaId ON ReceitaServico(ReceitaId);

    -- 2. Migrar dados existentes (se houver colunas antigas)
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdCategoriaServico')
    BEGIN
        INSERT INTO ReceitaServico (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId)
        SELECT NEWID(), Id, IdCategoriaServico, IdSubcategoriaServico
        FROM Receita
        WHERE IdCategoriaServico IS NOT NULL;
    END
END

-- 3. Remover colunas antigas (se existirem)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_CategoriaServico')
    ALTER TABLE Receita DROP CONSTRAINT FK_Receita_CategoriaServico;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receita_SubcategoriaServico')
    ALTER TABLE Receita DROP CONSTRAINT FK_Receita_SubcategoriaServico;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdCategoriaServico')
    ALTER TABLE Receita DROP COLUMN IdCategoriaServico;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Receita') AND name = 'IdSubcategoriaServico')
    ALTER TABLE Receita DROP COLUMN IdSubcategoriaServico;
