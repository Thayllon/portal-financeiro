-- Contrato recorrente: EhRecorrente + IdRegra + geração de receitas.
-- Idempotente para bancos já criados (001 já contém colunas no from-scratch).

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='contrato' AND column_name='ehrecorrente') THEN
        ALTER TABLE Contrato ADD COLUMN EhRecorrente BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='contrato' AND column_name='idregra') THEN
        ALTER TABLE Contrato ADD COLUMN IdRegra UUID NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='fk_contrato_regra') THEN
        ALTER TABLE Contrato ADD CONSTRAINT FK_Contrato_Regra FOREIGN KEY (IdRegra) REFERENCES RegraReceita(Id);
    END IF;
END $$;
