-- Contratos: tabela + vínculo em Receita + módulo de permissão.
-- Idempotente para bancos já criados antes desta migração (001 já contém o schema final).
CREATE TABLE IF NOT EXISTS Contrato (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    IdCliente UUID NOT NULL,
    Valor NUMERIC(18,2) NOT NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CONSTRAINT FK_Contrato_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Contrato_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id)
);

CREATE INDEX IF NOT EXISTS IX_Contrato_Usuario ON Contrato(IdUsuario);

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='receita' AND column_name='idcontrato') THEN
        ALTER TABLE Receita ADD COLUMN IdContrato UUID NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='fk_receita_contrato') THEN
        ALTER TABLE Receita ADD CONSTRAINT FK_Receita_Contrato FOREIGN KEY (IdContrato) REFERENCES Contrato(Id);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS IX_Receita_Contrato ON Receita(IdContrato);

-- Garante o módulo 'contratos' para usuários que ainda não o possuem.
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), Id, 'contratos', 0
FROM Usuario
WHERE Id NOT IN (
    SELECT UsuarioId FROM PermissaoUsuario WHERE Modulo = 'contratos'
);
