-- Fluxo adicional de despesa: Cliente + Categoria de Servico
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='despesa' AND column_name='idcliente') THEN
        ALTER TABLE Despesa ADD COLUMN IdCliente UUID NULL REFERENCES Pessoa(Id);
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS DespesaServico (
    Id UUID PRIMARY KEY,
    DespesaId UUID NOT NULL REFERENCES Despesa(Id) ON DELETE CASCADE,
    CategoriaServicoId UUID NOT NULL REFERENCES CategoriaServico(Id),
    SubcategoriaServicoId UUID NULL REFERENCES CategoriaServico(Id)
);
CREATE INDEX IF NOT EXISTS IX_DespesaServico_DespesaId ON DespesaServico(DespesaId);
