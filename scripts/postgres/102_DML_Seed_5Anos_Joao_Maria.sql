-- 102_DML_Seed_5Anos_Joao_Maria.sql — PostgreSQL (Neon)
-- Cria 5 ANOS de dados (60 meses) para João Souza (joao@portal.com) e Maria Silva (maria@portal.com).
-- IDEMPOTENTE: apaga apenas os lançamentos gerados por este script e recria.
-- Execute APÓS o DDL (100_DDL_AtualizarEstrutura.sql).
--
-- Uso no Neon:
--   psql "postgresql://USER:PASS@HOST/neondb?sslmode=require" -f scripts/postgres/102_DML_Seed_5Anos_Joao_Maria.sql
--   ou cole no SQL Editor do console.neon.tech
--
-- Período: 60 meses terminando no mês atual (ex.: se hoje é 2026-09, gera 2021-10 a 2026-09).
-- Valores incluem previsão para o mês atual/futuro via regra, e realizado para meses passados.

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

DO $$
DECLARE
    v_maria_id UUID;
    v_joao_id UUID;
    v_maria_conta UUID;
    v_joao_conta UUID;
    v_maria_cat_rec UUID;
    v_maria_cat_desp UUID;
    v_joao_cat_rec UUID;
    v_joao_cat_desp UUID;
    v_data DATE;
    v_status INT;
    v_data_realiz TIMESTAMP;
BEGIN
    -- ============================================================
    -- 1. Garante usuários (se não existirem, cria)
    -- ============================================================
    SELECT Id INTO v_maria_id FROM Usuario WHERE Email = 'maria@portal.com';
    IF v_maria_id IS NULL THEN
        SELECT Id INTO v_maria_id FROM Usuario WHERE Email = 'maria@demo.com';
    END IF;
    IF v_maria_id IS NULL THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'Maria Silva', 'maria@portal.com', '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING Id INTO v_maria_id;
        INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
        VALUES (gen_random_uuid(), v_maria_id, 'parcerias', 1), (gen_random_uuid(), v_maria_id, 'contratos', 1);
    END IF;

    SELECT Id INTO v_joao_id FROM Usuario WHERE Email = 'joao@portal.com';
    IF v_joao_id IS NULL THEN
        SELECT Id INTO v_joao_id FROM Usuario WHERE Email = 'joao@demo.com';
    END IF;
    IF v_joao_id IS NULL THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'João Souza', 'joao@portal.com', '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING Id INTO v_joao_id;
        INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
        VALUES (gen_random_uuid(), v_joao_id, 'parcerias', 1), (gen_random_uuid(), v_joao_id, 'contratos', 1);
    END IF;

    -- Garante que e-mail está atualizado para portal.com
    UPDATE Usuario SET Email = 'maria@portal.com', SenhaHash = '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', DataAlteracao = CURRENT_TIMESTAMP WHERE Id = v_maria_id AND Email <> 'maria@portal.com';
    UPDATE Usuario SET Email = 'joao@portal.com', SenhaHash = '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', DataAlteracao = CURRENT_TIMESTAMP WHERE Id = v_joao_id AND Email <> 'joao@portal.com';

    -- Garante admin (evita base sem admin após TRUNCATE)
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com') THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
    END IF;

    -- ============================================================
    -- 2. Garante Conta Bancária
    -- ============================================================
    SELECT Id INTO v_maria_conta FROM ContaBancaria WHERE IdUsuario = v_maria_id AND Ativo = TRUE LIMIT 1;
    IF v_maria_conta IS NULL THEN
        INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Conta Principal', 'Nubank', 1, TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_conta;
    END IF;

    SELECT Id INTO v_joao_conta FROM ContaBancaria WHERE IdUsuario = v_joao_id AND Ativo = TRUE LIMIT 1;
    IF v_joao_conta IS NULL THEN
        INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Conta Principal', 'Inter', 1, TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_conta;
    END IF;

    -- ============================================================
    -- 3. Garante Categorias
    -- ============================================================
    SELECT Id INTO v_maria_cat_rec FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Vendas' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_rec IS NULL THEN
        INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_rec;
    END IF;
    SELECT Id INTO v_maria_cat_desp FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Operacional' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_desp IS NULL THEN
        INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Operacional', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_desp;
    END IF;

    SELECT Id INTO v_joao_cat_rec FROM CategoriaReceita WHERE IdUsuario = v_joao_id AND Nome = 'Vendas' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_rec IS NULL THEN
        INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_rec;
    END IF;
    SELECT Id INTO v_joao_cat_desp FROM CategoriaDespesa WHERE IdUsuario = v_joao_id AND Nome = 'Operacional' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_desp IS NULL THEN
        INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Operacional', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_desp;
    END IF;

    -- ============================================================
    -- 4. Limpa lançamentos antigos deste seed (idempotência) — apaga 1Ano e 5Anos
    -- ============================================================
    DELETE FROM ReceitaServico WHERE ReceitaId IN (SELECT Id FROM Receita WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%'));
    DELETE FROM DespesaServico WHERE DespesaId IN (SELECT Id FROM Despesa WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%'));
    DELETE FROM Receita WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%');
    DELETE FROM Despesa WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%');

    -- ============================================================
    -- 5. Gera 60 meses (5 anos) para MARIA SILVA (volume maior)
    -- ============================================================
    FOR i IN 0..59 LOOP
        v_data := (date_trunc('month', CURRENT_DATE) - INTERVAL '59 months' + (i || ' months')::interval)::date;
        -- Meses passados = Realizado (2), mês atual/futuro = Realizado também para simplificar
        v_status := 2;
        v_data_realiz := (v_data + INTERVAL '5 days')::timestamp;

        -- Receita Maria: 6.5k a 8.5k variando por mês
        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (
            gen_random_uuid(), v_maria_id,
            'Receita Mensal - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'),
            (6500 + (i * 137 % 1800) + CASE WHEN i % 3 = 0 THEN 400 ELSE 0 END)::numeric(18,2),
            v_data, v_maria_conta, v_maria_cat_rec, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );

        -- Despesa Maria: 2.2k a 3.8k
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (
            gen_random_uuid(), v_maria_id,
            'Despesa Mensal - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'),
            (2200 + (i * 89 % 1400) + CASE WHEN i % 4 = 0 THEN 300 ELSE 0 END)::numeric(18,2),
            v_data, v_maria_conta, v_maria_cat_desp, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
    END LOOP;

    -- ============================================================
    -- 6. Gera 60 meses (5 anos) para JOÃO SOUZA (volume menor)
    -- ============================================================
    FOR i IN 0..59 LOOP
        v_data := (date_trunc('month', CURRENT_DATE) - INTERVAL '59 months' + (i || ' months')::interval)::date;
        v_status := 2;
        v_data_realiz := (v_data + INTERVAL '5 days')::timestamp;

        -- Receita João: 3.8k a 5.5k
        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (
            gen_random_uuid(), v_joao_id,
            'Receita Mensal - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'),
            (3800 + (i * 97 % 1500) + CASE WHEN i % 3 = 1 THEN 250 ELSE 0 END)::numeric(18,2),
            v_data, v_joao_conta, v_joao_cat_rec, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );

        -- Despesa João: 1.5k a 2.8k
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (
            gen_random_uuid(), v_joao_id,
            'Despesa Mensal - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'),
            (1500 + (i * 73 % 1100) + CASE WHEN i % 5 = 0 THEN 200 ELSE 0 END)::numeric(18,2),
            v_data, v_joao_conta, v_joao_cat_desp, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
    END LOOP;

    RAISE NOTICE 'Seed 5 anos concluído: Maria=% João=%', v_maria_id, v_joao_id;
END $$;
