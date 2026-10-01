-- 103_CheckEnums.sql — PostgreSQL (Neon)
-- CHECK constraints documentando os valores válidos de cada "tipo" (espelho de scripts/sqlserver/018_CheckEnums.sql).
--   contabancaria.tipo .......... 1=PF, 2=PJ (TipoConta)
--   pessoa.tipo ................. 1=Cliente, 2=Parceiro (TipoPessoa)
--   permissaousuario.nivel ...... 0=Nenhum, 1=Leitura, 2=Escrita (NivelPermissao)
--   receita/despesa.status ...... 1=Pendente, 2=Realizado (StatusMensal)
--   categoriahistorico.tipocategoria .. 1=Receita, 2=Despesa, 3=Serviços (ETipoCategoria)
--   categoriahistorico.acao ..... 1=Criado, 2=Editado, 3=Excluído (EAcaoCategoriaHistorico)
-- IDEMPOTENTE: cria cada constraint somente se ainda não existir.

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_contabancaria_tipo') THEN
        ALTER TABLE contabancaria ADD CONSTRAINT ck_contabancaria_tipo CHECK (tipo IN (1, 2));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_pessoa_tipo') THEN
        ALTER TABLE pessoa ADD CONSTRAINT ck_pessoa_tipo CHECK (tipo IN (1, 2));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_permissaousuario_nivel') THEN
        ALTER TABLE permissaousuario ADD CONSTRAINT ck_permissaousuario_nivel CHECK (nivel IN (0, 1, 2));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_receita_status') THEN
        ALTER TABLE receita ADD CONSTRAINT ck_receita_status CHECK (status IN (1, 2));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_despesa_status') THEN
        ALTER TABLE despesa ADD CONSTRAINT ck_despesa_status CHECK (status IN (1, 2));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_categoriahistorico_tipo') THEN
        ALTER TABLE categoriahistorico ADD CONSTRAINT ck_categoriahistorico_tipo CHECK (tipocategoria IN (1, 2, 3));
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_categoriahistorico_acao') THEN
        ALTER TABLE categoriahistorico ADD CONSTRAINT ck_categoriahistorico_acao CHECK (acao IN (1, 2, 3));
    END IF;
END $$;
