-- Migration: Substituir colunas únicas de serviço por tabela de junção (muitos-para-muitos)

-- 1. Criar tabela de junção
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

-- 2. Migrar dados existentes (se houver)
INSERT INTO ReceitaServico (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId)
SELECT NEWID(), Id, IdCategoriaServico, IdSubcategoriaServico
FROM Receita
WHERE IdCategoriaServico IS NOT NULL;

-- 3. Remover colunas antigas (drop FKs primeiro)
ALTER TABLE Receita DROP CONSTRAINT FK_Receita_CategoriaServico;
ALTER TABLE Receita DROP CONSTRAINT FK_Receita_SubcategoriaServico;
ALTER TABLE Receita DROP COLUMN IdCategoriaServico;
ALTER TABLE Receita DROP COLUMN IdSubcategoriaServico;
