-- Migration: Criar tabela Parceria e vincular em Receita/Despesa

CREATE TABLE IF NOT EXISTS Parceria (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    IdParceiro UUID NOT NULL,
    IdCliente UUID NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Parceria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Parceria_Parceiro FOREIGN KEY (IdParceiro) REFERENCES Pessoa(Id),
    CONSTRAINT FK_Parceria_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
);

CREATE INDEX IF NOT EXISTS IX_Parceria_Usuario ON Parceria(IdUsuario);

ALTER TABLE Receita ADD COLUMN IF NOT EXISTS IdParceria UUID NULL;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Receita_Parceria') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id);
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS IX_Receita_Parceria ON Receita(IdParceria);

ALTER TABLE Despesa ADD COLUMN IF NOT EXISTS IdParceria UUID NULL;
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Despesa_Parceria') THEN
        ALTER TABLE Despesa ADD CONSTRAINT FK_Despesa_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id);
    END IF;
END $$;
CREATE INDEX IF NOT EXISTS IX_Despesa_Parceria ON Despesa(IdParceria);
