-- 100_SeedDemo.sql — Dados fake para demonstração (idempotente)
-- 3 usuários: admin@portal.com (zerado), maria@demo.com (empresa boa, fluxo adicional ON), joao@demo.com (capenga, fluxo OFF)
-- Período: 2024-01 até 2026-09 | Senha de todos: senhasenha
-- Hash PBKDF2 correspondente a "senhasenha" (mesmo do 099_SeedBase.sql)
-- Executar APÓS 001_CriarTabelas.sql e 099_SeedBase.sql

-- ============================================================
-- 1) Usuários demo (Maria e João) — idempotente
-- ============================================================
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), 'Maria Silva', 'maria@demo.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'maria@demo.com');

INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), 'João Souza', 'joao@demo.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', FALSE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'joao@demo.com');

-- Garante permissão parcerias para os demos
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), u.Id, 'parcerias', 1 FROM Usuario u
WHERE u.Email IN ('maria@demo.com','joao@demo.com')
AND NOT EXISTS (SELECT 1 FROM PermissaoUsuario p WHERE p.UsuarioId = u.Id AND p.Modulo = 'parcerias');

-- Fluxo adicional habilitado SOMENTE para Maria
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), u.Id, 'fluxo-adicional-receita', 1 FROM Usuario u WHERE u.Email = 'maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM PermissaoUsuario p WHERE p.UsuarioId = u.Id AND p.Modulo = 'fluxo-adicional-receita');

INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), u.Id, 'fluxo-adicional-despesa', 1 FROM Usuario u WHERE u.Email = 'maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM PermissaoUsuario p WHERE p.UsuarioId = u.Id AND p.Modulo = 'fluxo-adicional-despesa');

-- ============================================================
-- 2) Contas bancárias
-- ============================================================
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Banco Premium', 'Nubank', 2, TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM ContaBancaria c WHERE c.IdUsuario = u.Id AND c.Nome='Banco Premium');

INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Carteira', 'Inter', 1, FALSE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM ContaBancaria c WHERE c.IdUsuario = u.Id AND c.Nome='Carteira');

INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Banco Simples', 'Caixa', 1, TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM ContaBancaria c WHERE c.IdUsuario = u.Id AND c.Nome='Banco Simples');

-- Admin garante ao menos 1 conta para UI não vazia (mas sem lançamentos)
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Conta Principal', 'Banco do Brasil', 1, TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='admin@portal.com'
AND NOT EXISTS (SELECT 1 FROM ContaBancaria c WHERE c.IdUsuario = u.Id);

-- ============================================================
-- 3) Pessoas (clientes / parceiros)
-- ============================================================
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Empresa Alpha LTDA', '(11) 99999-0001', 1, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Empresa Alpha LTDA');
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Beta Comércio SA', '(11) 99999-0002', 1, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Beta Comércio SA');
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Carlos Parceiro', '(11) 98888-0001', 2, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Carlos Parceiro');
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Ana Parceira', '(11) 98888-0002', 2, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Ana Parceira');

INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Cliente Capenga ME', '(21) 97777-0001', 1, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Cliente Capenga ME');
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Parceiro Local', '(21) 97777-0002', 2, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM Pessoa p WHERE p.IdUsuario=u.Id AND p.Nome='Parceiro Local');

-- ============================================================
-- 4) Parceria (somente Maria — empresa boa usa parceria)
-- ============================================================
INSERT INTO Parceria (Id, IdUsuario, Nome, IdParceiro, IdCliente, Valor, PercentualParceiro, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Parceria Alpha-Carlos', par.Id, cli.Id, 50000, 15, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM Usuario u
JOIN Pessoa cli ON cli.IdUsuario=u.Id AND cli.Nome='Empresa Alpha LTDA'
JOIN Pessoa par ON par.IdUsuario=u.Id AND par.Nome='Carlos Parceiro'
WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM Parceria p2 WHERE p2.IdUsuario=u.Id AND p2.Nome='Parceria Alpha-Carlos');

-- ============================================================
-- 5) Categorias — Maria (completas)
-- ============================================================
-- Receita pais
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Vendas');
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Consultoria', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Consultoria');
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Serviços Premium', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Serviços Premium');
-- Receita subs
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Recorrente', pai.Id, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u JOIN CategoriaReceita pai ON pai.IdUsuario=u.Id AND pai.Nome='Vendas' WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Recorrente');
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Suporte', pai.Id, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u JOIN CategoriaReceita pai ON pai.IdUsuario=u.Id AND pai.Nome='Consultoria' WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Suporte');

-- Despesa pais
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Operacional', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Operacional');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Marketing', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Marketing');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Administrativo', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Administrativo');
-- Despesa subs
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Ads', pai.Id, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u JOIN CategoriaDespesa pai ON pai.IdUsuario=u.Id AND pai.Nome='Marketing' WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Ads');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Escritório', pai.Id, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u JOIN CategoriaDespesa pai ON pai.IdUsuario=u.Id AND pai.Nome='Administrativo' WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Escritório');

-- Serviço
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Desenvolvimento', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaServico c WHERE c.IdUsuario=u.Id AND c.Nome='Desenvolvimento');
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Design', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaServico c WHERE c.IdUsuario=u.Id AND c.Nome='Design');
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Consultoria Tec', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='maria@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaServico c WHERE c.IdUsuario=u.Id AND c.Nome='Consultoria Tec');

-- ============================================================
-- 6) Categorias — João (simples, capenga) e Admin (básico)
-- ============================================================
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Serviços', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Serviços');
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Vendas');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Custos Fixos', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Custos Fixos');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Insumos', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='joao@demo.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Insumos');

INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Vendas', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='admin@portal.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaReceita c WHERE c.IdUsuario=u.Id AND c.Nome='Vendas');
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Operacional', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='admin@portal.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa c WHERE c.IdUsuario=u.Id AND c.Nome='Operacional');
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'Geral', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP FROM Usuario u WHERE u.Email='admin@portal.com'
AND NOT EXISTS (SELECT 1 FROM CategoriaServico c WHERE c.IdUsuario=u.Id AND c.Nome='Geral');

-- ============================================================
-- 7) Lançamentos 2024-01 a 2026-09 — Maria (empresa boa) e João (capenga)
-- Gera via PL/pgSQL de forma idempotente (só se ainda não houver lançamentos)
-- ============================================================
DO $$
DECLARE
  maria_id UUID; joao_id UUID;
  maria_conta1 UUID; maria_conta2 UUID; joao_conta UUID;
  maria_cli1 UUID; maria_cli2 UUID; maria_parceria_id UUID;
  maria_cat_rec_vendas UUID; maria_cat_rec_consult UUID; maria_cat_rec_serv UUID;
  maria_cat_desp_op UUID; maria_cat_desp_mkt UUID; maria_cat_desp_adm UUID;
  maria_cat_serv_dev UUID; maria_cat_serv_design UUID;
  joao_cat_rec_serv UUID; joao_cat_rec_vendas UUID; joao_cat_desp_fixos UUID; joao_cat_desp_insumos UUID;
  v_ano INT; v_mes INT; v_dia INT;
  v_valor NUMERIC(18,2); v_status INT; v_data_real TIMESTAMP;
  v_desc TEXT[] := ARRAY['Venda produto','Consultoria mensal','Projeto Alpha','Licenciamento','Manutenção','Serviço avulso','Contrato Beta','Entrega sprint'];
  v_desp_desc TEXT[] := ARRAY['Aluguel','Folha pagamento','Marketing Ads','Ferramentas','Contador','Energia','Internet','Manutenção','Combustível','Material escritório'];
  r INT; i INT;
BEGIN
  SELECT Id INTO maria_id FROM Usuario WHERE Email='maria@demo.com';
  SELECT Id INTO joao_id FROM Usuario WHERE Email='joao@demo.com';
  IF maria_id IS NULL OR joao_id IS NULL THEN RAISE NOTICE 'Usuarios demo não encontrados'; RETURN; END IF;

  -- se já tem lançamentos, não recria
  IF EXISTS (SELECT 1 FROM Receita WHERE IdUsuario = maria_id) THEN RAISE NOTICE 'Seed demo já aplicado — pulando'; RETURN; END IF;

  SELECT Id INTO maria_conta1 FROM ContaBancaria WHERE IdUsuario=maria_id AND Nome='Banco Premium';
  SELECT Id INTO maria_conta2 FROM ContaBancaria WHERE IdUsuario=maria_id AND Nome='Carteira';
  SELECT Id INTO joao_conta FROM ContaBancaria WHERE IdUsuario=joao_id AND Nome='Banco Simples';
  SELECT Id INTO maria_cli1 FROM Pessoa WHERE IdUsuario=maria_id AND Nome='Empresa Alpha LTDA';
  SELECT Id INTO maria_cli2 FROM Pessoa WHERE IdUsuario=maria_id AND Nome='Beta Comércio SA';
  SELECT Id INTO maria_parceria_id FROM Parceria WHERE IdUsuario=maria_id AND Nome='Parceria Alpha-Carlos';

  SELECT Id INTO maria_cat_rec_vendas FROM CategoriaReceita WHERE IdUsuario=maria_id AND Nome='Vendas';
  SELECT Id INTO maria_cat_rec_consult FROM CategoriaReceita WHERE IdUsuario=maria_id AND Nome='Consultoria';
  SELECT Id INTO maria_cat_rec_serv FROM CategoriaReceita WHERE IdUsuario=maria_id AND Nome='Serviços Premium';
  SELECT Id INTO maria_cat_desp_op FROM CategoriaDespesa WHERE IdUsuario=maria_id AND Nome='Operacional';
  SELECT Id INTO maria_cat_desp_mkt FROM CategoriaDespesa WHERE IdUsuario=maria_id AND Nome='Marketing';
  SELECT Id INTO maria_cat_desp_adm FROM CategoriaDespesa WHERE IdUsuario=maria_id AND Nome='Administrativo';
  SELECT Id INTO maria_cat_serv_dev FROM CategoriaServico WHERE IdUsuario=maria_id AND Nome='Desenvolvimento';
  SELECT Id INTO maria_cat_serv_design FROM CategoriaServico WHERE IdUsuario=maria_id AND Nome='Design';

  SELECT Id INTO joao_cat_rec_serv FROM CategoriaReceita WHERE IdUsuario=joao_id AND Nome='Serviços';
  SELECT Id INTO joao_cat_rec_vendas FROM CategoriaReceita WHERE IdUsuario=joao_id AND Nome='Vendas';
  SELECT Id INTO joao_cat_desp_fixos FROM CategoriaDespesa WHERE IdUsuario=joao_id AND Nome='Custos Fixos';
  SELECT Id INTO joao_cat_desp_insumos FROM CategoriaDespesa WHERE IdUsuario=joao_id AND Nome='Insumos';

  -- Loop meses
  FOR v_ano IN 2024..2026 LOOP
    FOR v_mes IN 1..12 LOOP
      IF v_ano=2026 AND v_mes>9 THEN EXIT; END IF;

      -- ===== MARIA: 3 a 5 receitas por mês, valores altos (média 15-22k total) =====
      FOR i IN 1..(3 + (random()*2)::int) LOOP
        v_dia := 1 + (random()*27)::int;
        -- valor 2500 a 6500 por lançamento
        v_valor := 2500 + (random()*4000)::int;
        -- status: 70% realizado se mês passado, 30% se mês atual/futuro
        IF make_date(v_ano, v_mes, 15) < date_trunc('month', CURRENT_DATE) THEN
          v_status := CASE WHEN random()<0.75 THEN 2 ELSE 1 END;
        ELSE
          v_status := CASE WHEN random()<0.35 THEN 2 ELSE 1 END;
        END IF;
        v_data_real := CASE WHEN v_status=2 THEN make_date(v_ano, v_mes, v_dia) + (random()*3||' days')::interval ELSE NULL END;

        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, IdParceiro, IdCliente, IdParceria, Status, DataRealizacao, IdRegra, Ativo, DataCadastro, DataAlteracao)
        VALUES (
          gen_random_uuid(), maria_id,
          v_desc[1 + (random()*7)::int] || ' ' || v_mes || '/' || v_ano,
          v_valor,
          make_date(v_ano, v_mes, v_dia),
          CASE WHEN random()<0.7 THEN maria_conta1 ELSE maria_conta2 END,
          (ARRAY[maria_cat_rec_vendas, maria_cat_rec_consult, maria_cat_rec_serv])[1 + (random()*2)::int],
          NULL, NULL,
          CASE WHEN random()<0.4 THEN maria_cli1 WHEN random()<0.5 THEN maria_cli2 ELSE NULL END,
          CASE WHEN random()<0.35 THEN maria_parceria_id ELSE NULL END,
          v_status, v_data_real, NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
      END LOOP;

      -- Maria despesas: 4 a 6 por mês, ~40-45% da receita total do mês
      FOR i IN 1..(4 + (random()*2)::int) LOOP
        v_dia := 1 + (random()*27)::int;
        v_valor := 900 + (random()*2200)::int;
        IF make_date(v_ano, v_mes, 15) < date_trunc('month', CURRENT_DATE) THEN
          v_status := CASE WHEN random()<0.8 THEN 2 ELSE 1 END;
        ELSE
          v_status := CASE WHEN random()<0.4 THEN 2 ELSE 1 END;
        END IF;
        v_data_real := CASE WHEN v_status=2 THEN make_date(v_ano, v_mes, v_dia) ELSE NULL END;

        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, IdRegra, IdReceitaOrigem, IdParceria, IdCliente, Ativo, DataCadastro, DataAlteracao)
        VALUES (
          gen_random_uuid(), maria_id,
          v_desp_desc[1 + (random()*9)::int] || ' ' || v_mes || '/' || v_ano,
          v_valor,
          make_date(v_ano, v_mes, v_dia),
          CASE WHEN random()<0.6 THEN maria_conta1 ELSE maria_conta2 END,
          (ARRAY[maria_cat_desp_op, maria_cat_desp_mkt, maria_cat_desp_adm])[1 + (random()*2)::int],
          NULL, v_status, v_data_real, NULL, NULL,
          CASE WHEN random()<0.2 THEN maria_parceria_id ELSE NULL END,
          CASE WHEN random()<0.25 THEN maria_cli1 ELSE NULL END,
          TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
      END LOOP;

      -- ===== JOÃO: 2 a 3 receitas por mês, valores baixos (3-6k total) =====
      FOR i IN 1..(2 + (random()*1)::int) LOOP
        v_dia := 1 + (random()*27)::int;
        v_valor := 900 + (random()*1800)::int;
        IF make_date(v_ano, v_mes, 15) < date_trunc('month', CURRENT_DATE) THEN
          v_status := CASE WHEN random()<0.6 THEN 2 ELSE 1 END;
        ELSE
          v_status := CASE WHEN random()<0.25 THEN 2 ELSE 1 END;
        END IF;
        v_data_real := CASE WHEN v_status=2 THEN make_date(v_ano, v_mes, v_dia) ELSE NULL END;

        INSERT INTO Receita (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, IdParceiro, IdCliente, IdParceria, Status, DataRealizacao, IdRegra, Ativo, DataCadastro, DataAlteracao)
        VALUES (
          gen_random_uuid(), joao_id,
          v_desc[1 + (random()*7)::int] || ' ' || v_mes || '/' || v_ano,
          v_valor,
          make_date(v_ano, v_mes, v_dia),
          joao_conta,
          CASE WHEN random()<0.5 THEN joao_cat_rec_serv ELSE joao_cat_rec_vendas END,
          NULL, NULL, NULL, NULL, v_status, v_data_real, NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
      END LOOP;

      -- João despesas: 3 a 5 por mês, 85-110% da receita (capenga)
      FOR i IN 1..(3 + (random()*2)::int) LOOP
        v_dia := 1 + (random()*27)::int;
        v_valor := 700 + (random()*1500)::int;
        IF make_date(v_ano, v_mes, 15) < date_trunc('month', CURRENT_DATE) THEN
          v_status := CASE WHEN random()<0.7 THEN 2 ELSE 1 END;
        ELSE
          v_status := CASE WHEN random()<0.3 THEN 2 ELSE 1 END;
        END IF;
        v_data_real := CASE WHEN v_status=2 THEN make_date(v_ano, v_mes, v_dia) ELSE NULL END;

        INSERT INTO Despesa (Id, IdUsuario, Descricao, Valor, Data, IdConta, IdCategoria, IdSubcategoria, Status, DataRealizacao, IdRegra, IdReceitaOrigem, IdParceria, IdCliente, Ativo, DataCadastro, DataAlteracao)
        VALUES (
          gen_random_uuid(), joao_id,
          v_desp_desc[1 + (random()*9)::int] || ' ' || v_mes || '/' || v_ano,
          v_valor,
          make_date(v_ano, v_mes, v_dia),
          joao_conta,
          CASE WHEN random()<0.5 THEN joao_cat_desp_fixos ELSE joao_cat_desp_insumos END,
          NULL, v_status, v_data_real, NULL, NULL, NULL, NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
        );
      END LOOP;

    END LOOP;
  END LOOP;

  -- Fluxo adicional: para ~35% das receitas da Maria, criar ReceitaServico (contrato)
  INSERT INTO ReceitaServico (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId)
  SELECT gen_random_uuid(), r.Id, maria_cat_serv_dev, NULL
  FROM Receita r WHERE r.IdUsuario=maria_id AND r.IdParceria IS NOT NULL AND random()<0.6
  LIMIT 40;
  INSERT INTO ReceitaServico (Id, ReceitaId, CategoriaServicoId, SubcategoriaServicoId)
  SELECT gen_random_uuid(), r.Id, maria_cat_serv_design, NULL
  FROM Receita r WHERE r.IdUsuario=maria_id AND r.IdCliente IS NOT NULL AND r.Id NOT IN (SELECT ReceitaId FROM ReceitaServico)
  LIMIT 20;

  -- Regras recorrentes para Maria (alimentam previsão do dashboard)
  INSERT INTO RegraReceita (Id, IdUsuario, Descricao, Valor, Dia, DiaUtil, IdCategoria, IdConta, DataInicio, DataFim, Ativo, DataCadastro, DataAlteracao)
  SELECT gen_random_uuid(), maria_id, 'Mensalidade Alpha', 8000, 10, FALSE, maria_cat_rec_vendas, maria_conta1, '2024-01-01'::timestamp, '2026-12-31'::timestamp, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
  WHERE NOT EXISTS (SELECT 1 FROM RegraReceita WHERE IdUsuario=maria_id AND Descricao='Mensalidade Alpha');
  INSERT INTO RegraDespesa (Id, IdUsuario, Descricao, Valor, Dia, DiaUtil, IdCategoria, IdConta, DataInicio, DataFim, Ativo, DataCadastro, DataAlteracao)
  SELECT gen_random_uuid(), maria_id, 'Aluguel escritório', 2500, 5, FALSE, maria_cat_desp_adm, maria_conta1, '2024-01-01'::timestamp, '2026-12-31'::timestamp, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
  WHERE NOT EXISTS (SELECT 1 FROM RegraDespesa WHERE IdUsuario=maria_id AND Descricao='Aluguel escritório');

END $$;

-- ReceitaServico/DespesaServico para despesas de contrato da Maria
INSERT INTO DespesaServico (Id, DespesaId, CategoriaServicoId, SubcategoriaServicoId)
SELECT gen_random_uuid(), d.Id, (SELECT Id FROM CategoriaServico WHERE IdUsuario=(SELECT Id FROM Usuario WHERE Email='maria@demo.com') AND Nome='Desenvolvimento' LIMIT 1), NULL
FROM Despesa d WHERE d.IdUsuario=(SELECT Id FROM Usuario WHERE Email='maria@demo.com') AND d.IdCliente IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM DespesaServico ds WHERE ds.DespesaId=d.Id)
LIMIT 15;
