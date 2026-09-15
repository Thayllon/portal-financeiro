ALTER TABLE Receita ADD COLUMN IF NOT EXISTS IdParceiro UUID NULL;
ALTER TABLE Receita ADD COLUMN IF NOT EXISTS IdCategoriaServico UUID NULL;
ALTER TABLE Receita ADD COLUMN IF NOT EXISTS IdSubcategoriaServico UUID NULL;
ALTER TABLE Receita ADD COLUMN IF NOT EXISTS IdCliente UUID NULL;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Receita_Parceiro') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Receita_CategoriaServico') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_CategoriaServico FOREIGN KEY (IdCategoriaServico) REFERENCES CategoriaServico(Id);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Receita_SubcategoriaServico') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_SubcategoriaServico FOREIGN KEY (IdSubcategoriaServico) REFERENCES CategoriaServico(Id);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Receita_Cliente') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);
    END IF;
END $$;
