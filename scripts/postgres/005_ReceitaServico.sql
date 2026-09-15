-- Migration: Substituir colunas únicas de serviço por tabela de junção (muitos-para-muitos)

-- 1. Criar tabela de junção
CREATE TABLE IF NOT EXISTS ReceitaServico (
    Id              UUID PRIMARY KEY,
    ReceitaId       UUID NOT NULL,
    CategoriaServicoId UUID NOT NULL,
    SubcategoriaServicoId UUID NULL,
    CONSTRAINT FK_ReceitaServico_Receita FOREIGN KEY (ReceitaId) REFERENCES Receita(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ReceitaServico_CategoriaServico FOREIGN KEY (CategoriaServicoId) REFERENCES CategoriaServico(Id),
    CONSTRAINT FK_ReceitaServico_SubcategoriaServico FOREIGN KEY (SubcategoriaServicoId) REFERENCES CategoriaServico(Id)
);

CREATE INDEX IX_ReceitaServico_ReceitaId ON ReceitaServico(ReceitaId);

-- 2. Migrar dados existentes (se houver)
INSERT INTO ReceitaServico (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId)
SELECT gen_random_uuid(), Id, IdCategoriaServico, IdSubcategoriaServico
FROM Receita
WHERE IdCategoriaServico IS NOT NULL
ON CONFLICT DO NOTHING;

-- 3. Remover colunas antigas
ALTER TABLE Receita DROP CONSTRAINT IF EXISTS FK_Receita_CategoriaServico;
ALTER TABLE Receita DROP CONSTRAINT IF EXISTS FK_Receita_SubcategoriaServico;
ALTER TABLE Receita DROP COLUMN IF EXISTS IdCategoriaServico;
ALTER TABLE Receita DROP COLUMN IF EXISTS IdSubcategoriaServico;
