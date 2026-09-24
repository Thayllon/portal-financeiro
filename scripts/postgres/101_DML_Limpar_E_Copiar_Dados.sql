-- 101_DML_Limpar_E_Copiar_Dados.sql — PostgreSQL (Neon)
-- ATENÇÃO: APAGA TODOS OS DADOS DE PROD e substitui por uma cópia do seu banco LOCAL.
-- DDL (estrutura) deve já estar atualizada — rode o 100_DDL_AtualizarEstrutura.sql ANTES.
--
-- COMO USAR:
--   1. No seu PC, gere o arquivo com os INSERTs do banco local:
--        powershell -ExecutionPolicy Bypass -File scripts/CopiarLocalParaProd.ps1
--      Isso cria "portal-financeiro-prod-restore.sql" na raiz do projeto.
--   2. Rode ESTE script no Neon (SQL Editor ou psql) — ele só faz o TRUNCATE.
--   3. Em seguida, rode o arquivo gerado no passo 1 no mesmo banco Neon.
--
-- ALTERNATIVA EM UM PASSO: rode direto o arquivo gerado (ele já contém BEGIN; TRUNCATE; INSERTs; COMMIT;)
--   psql "postgresql://USER:PASS@HOST/neondb?sslmode=require" -f portal-financeiro-prod-restore.sql
--
-- BACKUP: faça backup do Neon antes (Neon Console -> Branches -> Backup ou pg_dump).

BEGIN;

-- Ordem respeita FKs (filhas primeiro). CASCADE garante limpeza total.
TRUNCATE TABLE CategoriaHistorico CASCADE;
TRUNCATE TABLE ReceitaServico CASCADE;
TRUNCATE TABLE DespesaServico CASCADE;
TRUNCATE TABLE Receita CASCADE;
TRUNCATE TABLE Despesa CASCADE;
TRUNCATE TABLE RegraReceita CASCADE;
TRUNCATE TABLE RegraDespesa CASCADE;
TRUNCATE TABLE Parceria CASCADE;
TRUNCATE TABLE Contrato CASCADE;
TRUNCATE TABLE PermissaoUsuario CASCADE;
TRUNCATE TABLE CategoriaServico CASCADE;
TRUNCATE TABLE CategoriaDespesa CASCADE;
TRUNCATE TABLE CategoriaReceita CASCADE;
TRUNCATE TABLE Pessoa CASCADE;
TRUNCATE TABLE ContaBancaria CASCADE;
TRUNCATE TABLE Usuario CASCADE;

COMMIT;

-- Após este COMMIT, o banco está VAZIO.
-- Cole/Execute agora o conteúdo de "portal-financeiro-prod-restore.sql" (INSERTs).
-- Se usou a alternativa de um passo, ignore este arquivo e use só o .ps1.
