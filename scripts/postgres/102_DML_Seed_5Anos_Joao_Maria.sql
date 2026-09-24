-- 102_DML_Seed_5Anos_Joao_Maria.sql — PostgreSQL (Neon)
-- Cria 5 ANOS de dados RICOS (60 meses) para João Souza (joao@portal.com) e Maria Silva (maria@portal.com).
-- Inspirado nos dados reais de Alana/Thayllon (local) mas com valores e distribuição próprios — NÃO é cópia.
-- IDEMPOTENTE: apaga apenas os lançamentos gerados por este script e recria.
-- Execute APÓS o DDL (100_DDL_AtualizarEstrutura.sql).
--
-- Uso no Neon:
--   psql "postgresql://USER:PASS@HOST/neondb?sslmode=require" -f scripts/postgres/102_DML_Seed_5Anos_Joao_Maria.sql
--   ou cole no SQL Editor do console.neon.tech
--
-- Período: 60 meses terminando no mês atual (ex.: se hoje é 2026-09, gera 2021-10 a 2026-09).
-- Gera múltiplos lançamentos por mês com categorias variadas para preencher dashboard, distribuição e indicadores.

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

DO $$
DECLARE
    v_maria_id UUID;
    v_joao_id UUID;
    v_admin_id UUID;
    v_maria_conta UUID;
    v_joao_conta UUID;
    v_maria_cat_vendas UUID;
    v_maria_cat_empresa UUID;
    v_maria_cat_pessoal_rec UUID;
    v_maria_cat_pessoal_desp UUID;
    v_maria_cat_cnpj UUID;
    v_maria_cat_filhas UUID;
    v_maria_cat_carro UUID;
    v_maria_cat_casa UUID;
    v_maria_cat_alimentacao UUID;
    v_joao_cat_vendas UUID;
    v_joao_cat_servicos UUID;
    v_joao_cat_operacional UUID;
    v_joao_cat_alimentacao UUID;
    v_maria_sub_vendas_rec UUID;
    v_maria_sub_vendas_avulso UUID;
    v_maria_sub_pessoal_supermercado UUID;
    v_maria_sub_cnpj_impostos UUID;
    v_maria_sub_filhas_escola UUID;
    v_maria_sub_carro_combustivel UUID;
    v_joao_sub_vendas_recorrente UUID;
    v_joao_sub_operacional_manutencao UUID;
    v_data DATE;
    v_status INT;
    v_data_realiz TIMESTAMP;
    v_mes INT;
BEGIN
    -- 1. Garante usuários
    SELECT Id INTO v_admin_id FROM Usuario WHERE Email = 'admin@portal.com';
    IF v_admin_id IS NULL THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING Id INTO v_admin_id;
    END IF;

    SELECT Id INTO v_maria_id FROM Usuario WHERE Email = 'maria@portal.com';
    IF v_maria_id IS NULL THEN
        SELECT Id INTO v_maria_id FROM Usuario WHERE Email = 'maria@demo.com';
    END IF;
    IF v_maria_id IS NULL THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'Maria Silva', 'maria@portal.com', '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING Id INTO v_maria_id;
        INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel) VALUES (gen_random_uuid(), v_maria_id, 'parcerias', 1), (gen_random_uuid(), v_maria_id, 'contratos', 1);
    END IF;
    SELECT Id INTO v_joao_id FROM Usuario WHERE Email = 'joao@portal.com';
    IF v_joao_id IS NULL THEN
        SELECT Id INTO v_joao_id FROM Usuario WHERE Email = 'joao@demo.com';
    END IF;
    IF v_joao_id IS NULL THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'João Souza', 'joao@portal.com', '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        RETURNING Id INTO v_joao_id;
        INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel) VALUES (gen_random_uuid(), v_joao_id, 'parcerias', 1), (gen_random_uuid(), v_joao_id, 'contratos', 1);
    END IF;
    UPDATE Usuario SET Email = 'maria@portal.com', SenhaHash = '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', DataAlteracao = CURRENT_TIMESTAMP WHERE Id = v_maria_id AND Email <> 'maria@portal.com';
    UPDATE Usuario SET Email = 'joao@portal.com', SenhaHash = '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=', DataAlteracao = CURRENT_TIMESTAMP WHERE Id = v_joao_id AND Email <> 'joao@portal.com';
    IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com') THEN
        INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
    END IF;

    -- 2. Garante Contas
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

    -- 3. Garante Categorias (inspirado em Alana: Pessoal, CNPJ, Filhas, Carro, Casa, Alimentação)
    SELECT Id INTO v_maria_cat_vendas FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Vendas' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_vendas IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_vendas; END IF;
    SELECT Id INTO v_maria_cat_empresa FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Empresa' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_empresa IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Empresa', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_empresa; END IF;
    SELECT Id INTO v_maria_cat_pessoal_rec FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Pessoal' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_pessoal_rec IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Pessoal', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_pessoal_rec; END IF;

    SELECT Id INTO v_maria_cat_pessoal_desp FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Pessoal' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_pessoal_desp IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Pessoal', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_pessoal_desp; END IF;
    SELECT Id INTO v_maria_cat_cnpj FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'CNPJ' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_cnpj IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'CNPJ', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_cnpj; END IF;
    SELECT Id INTO v_maria_cat_filhas FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Filhas' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_filhas IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Filhas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_filhas; END IF;
    SELECT Id INTO v_maria_cat_carro FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Carro' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_carro IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Carro', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_carro; END IF;
    SELECT Id INTO v_maria_cat_casa FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Casa' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_casa IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Casa', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_casa; END IF;
    SELECT Id INTO v_maria_cat_alimentacao FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Alimentação' AND Ativo = TRUE LIMIT 1;
    IF v_maria_cat_alimentacao IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Alimentação', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_cat_alimentacao; END IF;

    SELECT Id INTO v_joao_cat_vendas FROM CategoriaReceita WHERE IdUsuario = v_joao_id AND Nome = 'Vendas' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_vendas IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_vendas; END IF;
    SELECT Id INTO v_joao_cat_servicos FROM CategoriaReceita WHERE IdUsuario = v_joao_id AND Nome = 'Serviços' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_servicos IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Serviços', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_servicos; END IF;
    SELECT Id INTO v_joao_cat_operacional FROM CategoriaDespesa WHERE IdUsuario = v_joao_id AND Nome = 'Operacional' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_operacional IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Operacional', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_operacional; END IF;
    SELECT Id INTO v_joao_cat_alimentacao FROM CategoriaDespesa WHERE IdUsuario = v_joao_id AND Nome = 'Alimentação' AND Ativo = TRUE LIMIT 1;
    IF v_joao_cat_alimentacao IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Alimentação', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_cat_alimentacao; END IF;

    -- Subcategorias (granularização para distribuição)
    SELECT Id INTO v_maria_sub_vendas_rec FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Recorrente' AND CategoriaPaiId = v_maria_cat_vendas LIMIT 1;
    IF v_maria_sub_vendas_rec IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Recorrente', v_maria_cat_vendas, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_vendas_rec; END IF;
    SELECT Id INTO v_maria_sub_vendas_avulso FROM CategoriaReceita WHERE IdUsuario = v_maria_id AND Nome = 'Avulso' AND CategoriaPaiId = v_maria_cat_vendas LIMIT 1;
    IF v_maria_sub_vendas_avulso IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Avulso', v_maria_cat_vendas, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_vendas_avulso; END IF;
    SELECT Id INTO v_maria_sub_pessoal_supermercado FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Supermercado' AND CategoriaPaiId = v_maria_cat_pessoal_desp LIMIT 1;
    IF v_maria_sub_pessoal_supermercado IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Supermercado', v_maria_cat_pessoal_desp, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_pessoal_supermercado; END IF;
    SELECT Id INTO v_maria_sub_cnpj_impostos FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Impostos' AND CategoriaPaiId = v_maria_cat_cnpj LIMIT 1;
    IF v_maria_sub_cnpj_impostos IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Impostos', v_maria_cat_cnpj, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_cnpj_impostos; END IF;
    SELECT Id INTO v_maria_sub_filhas_escola FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Escola' AND CategoriaPaiId = v_maria_cat_filhas LIMIT 1;
    IF v_maria_sub_filhas_escola IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Escola', v_maria_cat_filhas, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_filhas_escola; END IF;
    SELECT Id INTO v_maria_sub_carro_combustivel FROM CategoriaDespesa WHERE IdUsuario = v_maria_id AND Nome = 'Combustível' AND CategoriaPaiId = v_maria_cat_carro LIMIT 1;
    IF v_maria_sub_carro_combustivel IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_maria_id, 'Combustível', v_maria_cat_carro, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_maria_sub_carro_combustivel; END IF;
    SELECT Id INTO v_joao_sub_vendas_recorrente FROM CategoriaReceita WHERE IdUsuario = v_joao_id AND Nome = 'Recorrente' AND CategoriaPaiId = v_joao_cat_vendas LIMIT 1;
    IF v_joao_sub_vendas_recorrente IS NULL THEN INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Recorrente', v_joao_cat_vendas, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_sub_vendas_recorrente; END IF;
    SELECT Id INTO v_joao_sub_operacional_manutencao FROM CategoriaDespesa WHERE IdUsuario = v_joao_id AND Nome = 'Manutenção' AND CategoriaPaiId = v_joao_cat_operacional LIMIT 1;
    IF v_joao_sub_operacional_manutencao IS NULL THEN INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) VALUES (gen_random_uuid(), v_joao_id, 'Manutenção', v_joao_cat_operacional, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP) RETURNING Id INTO v_joao_sub_operacional_manutencao; END IF;

    -- 4. Limpa seed antigo
    DELETE FROM ReceitaServico WHERE ReceitaId IN (SELECT Id FROM Receita WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%'));
    DELETE FROM DespesaServico WHERE DespesaId IN (SELECT Id FROM Despesa WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%'));
    DELETE FROM Receita WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%');
    DELETE FROM Despesa WHERE IdUsuario IN (v_maria_id, v_joao_id) AND (Descricao LIKE '%[Seed 1Ano]%' OR Descricao LIKE '%[Seed 5Anos]%');

    -- 5. Gera 60 meses para MARIA (rico: 2-3 receitas + 5-6 despesas por mês, crescimento anual)
    FOR i IN 0..59 LOOP
        v_data := (date_trunc('month', CURRENT_DATE) - INTERVAL '59 months' + (i || ' months')::interval)::date;
        v_mes := EXTRACT(MONTH FROM v_data)::int;
        v_status := 2; v_data_realiz := (v_data + INTERVAL '5 days')::timestamp;

        -- Receitas Maria: 2 fixas + 1 extra a cada 3 meses (média 2.3/mês) — com subcategorias granulares
        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Venda Projeto - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (4200 + (i*13%800) + (v_mes*7%300) + CASE WHEN v_mes=12 THEN 600 ELSE 0 END)::numeric(18,2), v_data + (1 + i%5), v_maria_conta, v_maria_cat_vendas, CASE WHEN i%2=0 THEN v_maria_sub_vendas_rec ELSE v_maria_sub_vendas_avulso END, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Empresa - Recorrente Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (1800 + (i*11%400) + (i/12)*80)::numeric(18,2), v_data + (10 + i%10), v_maria_conta, v_maria_cat_empresa, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        IF i % 3 = 0 THEN
            INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
            VALUES (gen_random_uuid(), v_maria_id, 'Extra Pessoal - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (900 + (i*19%500))::numeric(18,2), v_data + (15 + i%7), v_maria_conta, v_maria_cat_pessoal_rec, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        END IF;

        -- Despesas Maria: 5 fixas + 1 extra (granular: subcategorias para distribuição)
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Pessoal - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (650 + (i*7%300) + (v_mes=7)::int*80)::numeric(18,2), v_data + 2, v_maria_conta, v_maria_cat_pessoal_desp, v_maria_sub_pessoal_supermercado, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'CNPJ - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (280 + (i*5%180) + CASE WHEN v_mes IN (1,7) THEN 120 ELSE 0 END)::numeric(18,2), v_data + 3, v_maria_conta, v_maria_cat_cnpj, v_maria_sub_cnpj_impostos, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Filhas - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (420 + (i*9%250))::numeric(18,2), v_data + 4, v_maria_conta, v_maria_cat_filhas, v_maria_sub_filhas_escola, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Carro - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (320 + (i*6%200))::numeric(18,2), v_data + 6, v_maria_conta, v_maria_cat_carro, v_maria_sub_carro_combustivel, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_maria_id, 'Casa - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (380 + (i*8%220) + CASE WHEN v_mes=8 THEN 150 ELSE 0 END)::numeric(18,2), v_data + 8, v_maria_conta, v_maria_cat_casa, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        IF i % 2 = 0 THEN
            INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
            VALUES (gen_random_uuid(), v_maria_id, 'Alimentação - Maria [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (180 + (i*11%120))::numeric(18,2), v_data + 12, v_maria_conta, v_maria_cat_alimentacao, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        END IF;
    END LOOP;

    -- 6. Gera 60 meses para JOÃO (enxuto: 1-2 receitas + 3-4 despesas por mês, com subcategorias)
    FOR i IN 0..59 LOOP
        v_data := (date_trunc('month', CURRENT_DATE) - INTERVAL '59 months' + (i || ' months')::interval)::date;
        v_status := 2; v_data_realiz := (v_data + INTERVAL '5 days')::timestamp;
        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Venda - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (3200 + (i*9%600) + (i/12)*60)::numeric(18,2), v_data + 2, v_joao_conta, v_joao_cat_vendas, v_joao_sub_vendas_recorrente, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        IF i % 4 = 0 THEN
            INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
            VALUES (gen_random_uuid(), v_joao_id, 'Serviços - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (1100 + (i*7%400))::numeric(18,2), v_data + 11, v_joao_conta, v_joao_cat_servicos, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        END IF;
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Operacional - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (520 + (i*6%280))::numeric(18,2), v_data + 3, v_joao_conta, v_joao_cat_operacional, v_joao_sub_operacional_manutencao, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
        VALUES (gen_random_uuid(), v_joao_id, 'Pessoal - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (380 + (i*8%200))::numeric(18,2), v_data + 5, v_joao_conta, v_joao_cat_operacional, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        IF i % 3 = 0 THEN
            INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, Ativo, DataCadastro, DataAlteracao)
            VALUES (gen_random_uuid(), v_joao_id, 'Alimentação - João [Seed 5Anos] ' || to_char(v_data, 'MM/YYYY'), (160 + (i*9%100))::numeric(18,2), v_data + 9, v_joao_conta, v_joao_cat_alimentacao, NULL, v_status, v_data_realiz, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
        END IF;
    END LOOP;

    RAISE NOTICE 'Seed 5 anos RICO concluído: Maria=% João=%', v_maria_id, v_joao_id;
END $$;
