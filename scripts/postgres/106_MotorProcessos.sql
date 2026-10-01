-- 106_MotorProcessos.sql — PostgreSQL (Neon / local)
-- Espelha scripts/sqlserver/021_MotorProcessos.sql e completa a estrutura para
-- ficar IDÊNTICA ao schema do SQL Server (LocalDB), que é a referência do projeto.
--
-- IDEMPOTENTE: pode rodar quantas vezes quiser, em banco novo ou desatualizado.
-- Cria: ModeloProcesso, ModeloEtapa, ModeloItem, Processo, ProcessoEtapa,
--       ProcessoEtapaItem e ProcessoAnexo; adiciona as colunas novas em Processo
--       e ProcessoEtapa; garante auditoria de ator, CHECKs, índices, o módulo
--       "processos" em PermissaoUsuario e o modelo "Regularização de Imóvel".
--
-- Em banco NOVO, a ordem completa é:
--   001_CriarTabelas.sql → 003 → 004 → 005 → 006 → 100 → 103 → 104 → 105 → 106
--
--   psql "postgresql://USER:PASS@HOST/db?sslmode=require" -f scripts/postgres/106_MotorProcessos.sql

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- Modelos de processo (templates reutilizáveis)
-- ============================================================

CREATE TABLE IF NOT EXISTS ModeloProcesso (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    CONSTRAINT FK_ModeloProcesso_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id)
);

CREATE INDEX IF NOT EXISTS IX_ModeloProcesso_Usuario ON ModeloProcesso(IdUsuario);

CREATE TABLE IF NOT EXISTS ModeloEtapa (
    Id UUID PRIMARY KEY,
    IdModeloProcesso UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    Ordem INT NOT NULL DEFAULT 1,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    CONSTRAINT FK_ModeloEtapa_Modelo FOREIGN KEY (IdModeloProcesso) REFERENCES ModeloProcesso(Id),
    CONSTRAINT CK_ModeloEtapa_Ordem CHECK (Ordem >= 1)
);

CREATE INDEX IF NOT EXISTS IX_ModeloEtapa_Modelo ON ModeloEtapa(IdModeloProcesso);

CREATE TABLE IF NOT EXISTS ModeloItem (
    Id UUID PRIMARY KEY,
    IdModeloEtapa UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    Obrigatorio BOOLEAN NOT NULL DEFAULT TRUE,
    ExigeAnexo BOOLEAN NOT NULL DEFAULT FALSE,
    Ordem INT NOT NULL DEFAULT 1,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    CONSTRAINT FK_ModeloItem_Etapa FOREIGN KEY (IdModeloEtapa) REFERENCES ModeloEtapa(Id),
    CONSTRAINT CK_ModeloItem_Ordem CHECK (Ordem >= 1)
);

CREATE INDEX IF NOT EXISTS IX_ModeloItem_Etapa ON ModeloItem(IdModeloEtapa);

-- ============================================================
-- Processo (fases e itens) — espelha o SQL Server
-- ============================================================

CREATE TABLE IF NOT EXISTS Processo (
    Id UUID PRIMARY KEY,
    IdUsuario UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    IdParceria UUID NULL,
    IdContrato UUID NULL,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    IdModeloProcesso UUID NULL,
    IdCliente UUID NULL,
    DataEncerramento TIMESTAMP NULL,
    CONSTRAINT FK_Processo_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    CONSTRAINT FK_Processo_Parceria FOREIGN KEY (IdParceria) REFERENCES Parceria(Id),
    CONSTRAINT FK_Processo_Contrato FOREIGN KEY (IdContrato) REFERENCES Contrato(Id),
    CONSTRAINT CK_Processo_Vinculo_Unico CHECK (NOT (IdParceria IS NOT NULL AND IdContrato IS NOT NULL))
);

DO $$ BEGIN
    IF to_regclass('processo') IS NOT NULL THEN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'processo' AND column_name = 'idmodeloprocesso') THEN
            ALTER TABLE processo ADD COLUMN idmodeloprocesso UUID NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'processo' AND column_name = 'idcliente') THEN
            ALTER TABLE processo ADD COLUMN idcliente UUID NULL;
        END IF;
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'processo' AND column_name = 'dataencerramento') THEN
            ALTER TABLE processo ADD COLUMN dataencerramento TIMESTAMP NULL;
        END IF;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS IX_Processo_Usuario ON Processo(IdUsuario);
CREATE INDEX IF NOT EXISTS IX_Processo_Parceria ON Processo(IdParceria);
CREATE INDEX IF NOT EXISTS IX_Processo_Contrato ON Processo(IdContrato);
CREATE INDEX IF NOT EXISTS IX_Processo_Modelo ON Processo(IdModeloProcesso);
CREATE INDEX IF NOT EXISTS IX_Processo_Cliente ON Processo(IdCliente);

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_processo_modelo') THEN
        ALTER TABLE processo ADD CONSTRAINT FK_Processo_Modelo FOREIGN KEY (IdModeloProcesso) REFERENCES ModeloProcesso(Id);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_processo_cliente') THEN
        ALTER TABLE processo ADD CONSTRAINT FK_Processo_Cliente FOREIGN KEY (IdCliente) REFERENCES Pessoa(Id);
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS ProcessoEtapa (
    Id UUID PRIMARY KEY,
    IdProcesso UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    Ordem INT NOT NULL DEFAULT 1,
    Concluida BOOLEAN NOT NULL DEFAULT FALSE,
    DataPrevista DATE NULL,
    DataConclusao TIMESTAMP NULL,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    DataInicio TIMESTAMP NULL,
    CONSTRAINT FK_ProcessoEtapa_Processo FOREIGN KEY (IdProcesso) REFERENCES Processo(Id),
    CONSTRAINT CK_ProcessoEtapa_Ordem CHECK (Ordem >= 1)
);

DO $$ BEGIN
    IF to_regclass('processoetapa') IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'processoetapa' AND column_name = 'datainicio') THEN
        ALTER TABLE processoetapa ADD COLUMN datainicio TIMESTAMP NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS IX_ProcessoEtapa_Processo ON ProcessoEtapa(IdProcesso);

CREATE TABLE IF NOT EXISTS ProcessoEtapaItem (
    Id UUID PRIMARY KEY,
    IdProcessoEtapa UUID NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Descricao VARCHAR(500) NULL,
    Obrigatorio BOOLEAN NOT NULL DEFAULT TRUE,
    ExigeAnexo BOOLEAN NOT NULL DEFAULT FALSE,
    Ordem INT NOT NULL DEFAULT 1,
    Concluida BOOLEAN NOT NULL DEFAULT FALSE,
    DataInicio TIMESTAMP NULL,
    DataConclusao TIMESTAMP NULL,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    CONSTRAINT FK_ProcessoEtapaItem_Etapa FOREIGN KEY (IdProcessoEtapa) REFERENCES ProcessoEtapa(Id),
    CONSTRAINT CK_ProcessoEtapaItem_Ordem CHECK (Ordem >= 1)
);

CREATE INDEX IF NOT EXISTS IX_ProcessoEtapaItem_Etapa ON ProcessoEtapaItem(IdProcessoEtapa);

CREATE TABLE IF NOT EXISTS ProcessoAnexo (
    Id UUID PRIMARY KEY,
    IdProcessoEtapaItem UUID NOT NULL,
    NomeArquivo VARCHAR(255) NOT NULL,
    ContentType VARCHAR(100) NULL,
    Tamanho BIGINT NOT NULL DEFAULT 0,
    DriveFileId VARCHAR(255) NULL,
    Url VARCHAR(500) NULL,
    DataCadastro TIMESTAMP NOT NULL,
    DataAlteracao TIMESTAMP NOT NULL,
    CriadoPor UUID NULL,
    AlteradoPor UUID NULL,
    CONSTRAINT FK_ProcessoAnexo_Item FOREIGN KEY (IdProcessoEtapaItem) REFERENCES ProcessoEtapaItem(Id)
);

CREATE INDEX IF NOT EXISTS IX_ProcessoAnexo_Item ON ProcessoAnexo(IdProcessoEtapaItem);

-- ============================================================
-- Auditoria de ator nas tabelas do motor (espelha 105_AuditoriaAtor.sql)
-- Sem FK para Usuario de propósito: o histórico sobrevive à exclusão do usuário.
-- ============================================================

DO $$ DECLARE
  t TEXT;
BEGIN
  FOREACH t IN ARRAY ARRAY['modeloprocesso','modeloetapa','modeloitem','processoetapaitem','processoanexo']
  LOOP
    IF to_regclass(format('%I', t)) IS NULL THEN CONTINUE; END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t AND column_name = 'criadopor') THEN
      EXECUTE format('ALTER TABLE %I ADD COLUMN criadopor UUID NULL', t);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = t AND column_name = 'alteradopor') THEN
      EXECUTE format('ALTER TABLE %I ADD COLUMN alteradopor UUID NULL', t);
    END IF;
  END LOOP;
END $$;

-- ============================================================
-- Módulo "processos" liberado (mesma regra do SQL Server)
-- ============================================================

INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), U.Id, 'processos', 0
FROM Usuario U
WHERE NOT EXISTS (
    SELECT 1 FROM PermissaoUsuario P
    WHERE P.UsuarioId = U.Id AND P.Modulo = 'processos'
);

-- ============================================================
-- Modelo padrão "Regularização de Imóvel" (espelha o seed do SQL Server)
-- ============================================================

INSERT INTO ModeloProcesso (Id, IdUsuario, Nome, Descricao, Ativo, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
SELECT gen_random_uuid(),
       U.Id,
       'Regularização de Imóvel',
       'Modelo padrão de regularização de imóvel: documental, execução, protocolo e entrega.',
       TRUE,
       CURRENT_TIMESTAMP,
       CURRENT_TIMESTAMP,
       U.Id,
       U.Id
FROM Usuario U
WHERE NOT EXISTS (
    SELECT 1 FROM ModeloProcesso M WHERE M.IdUsuario = U.Id AND M.Nome = 'Regularização de Imóvel'
);

INSERT INTO ModeloEtapa (Id, IdModeloProcesso, Nome, Descricao, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
SELECT gen_random_uuid(),
       M.Id,
       F.Nome,
       F.Descricao,
       F.Ordem,
       CURRENT_TIMESTAMP,
       CURRENT_TIMESTAMP,
       M.IdUsuario,
       M.IdUsuario
FROM ModeloProcesso M
CROSS JOIN (VALUES
    ('Documental', 'Documentação inicial: orçamento, contrato e coleta de dados do cliente.', 1),
    ('Mão na massa', 'Levantamento em loco e desenho do imóvel no software.', 2),
    ('Protocolar', 'Protocolo do processo na prefeitura.', 3),
    ('Etapa final', 'Emissão do documento e entrega do projeto ao cliente.', 4)
) AS F(Nome, Descricao, Ordem)
WHERE M.Nome = 'Regularização de Imóvel'
  AND NOT EXISTS (
      SELECT 1 FROM ModeloEtapa E WHERE E.IdModeloProcesso = M.Id AND E.Nome = F.Nome
  );

INSERT INTO ModeloItem (Id, IdModeloEtapa, Nome, Descricao, Obrigatorio, ExigeAnexo, Ordem, DataCadastro, DataAlteracao, CriadoPor, AlteradoPor)
SELECT gen_random_uuid(),
       E.Id,
       I.Nome,
       I.Descricao,
       I.Obrigatorio,
       I.ExigeAnexo,
       I.Ordem,
       CURRENT_TIMESTAMP,
       CURRENT_TIMESTAMP,
       M.IdUsuario,
       M.IdUsuario
FROM ModeloEtapa E
JOIN ModeloProcesso M ON M.Id = E.IdModeloProcesso
JOIN LATERAL (
    SELECT *
    FROM (VALUES
        ('Documental', 'Orçamento', 'Após mandar para o cliente, anexar o orçamento.', TRUE, TRUE, 1),
        ('Documental', 'Contrato', 'Após mandar para o cliente, anexar o contrato.', TRUE, TRUE, 2),
        ('Documental', 'Coleta de dados', 'Enviar ao cliente um formulário coletando todas as informações preliminares para dar início ao trabalho.', TRUE, FALSE, 3),
        ('Mão na massa', 'Levantamento em loco', 'Pedir as medidas do imóvel (máximo 3 visitas).', TRUE, FALSE, 1),
        ('Mão na massa', 'Desenho do imóvel', 'Desenhar o imóvel no software e anexar depois de pronto.', TRUE, TRUE, 2),
        ('Protocolar', 'Protocolo na prefeitura', 'Protocolar o processo na prefeitura.', TRUE, FALSE, 1),
        ('Etapa final', 'Emissão do documento', 'Emitir o documento final do processo.', TRUE, TRUE, 1),
        ('Etapa final', 'Entrega do projeto', 'Entregar o projeto ao cliente.', TRUE, FALSE, 2)
    ) AS v(Fase, Nome, Descricao, Obrigatorio, ExigeAnexo, Ordem)
    WHERE v.Fase = E.Nome
) AS I ON TRUE
WHERE M.Nome = 'Regularização de Imóvel'
  AND NOT EXISTS (
      SELECT 1 FROM ModeloItem MI WHERE MI.IdModeloEtapa = E.Id AND MI.Nome = I.Nome
  );