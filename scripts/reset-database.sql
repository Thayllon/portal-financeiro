-- Portal Financeiro - Reset de banco de desenvolvimento (SQL Server)
-- Mantem APENAS o usuario padrao admin@portal.com.
-- TUDO o resto e removido, inclusive categorias (ate as de referencia do admin,
-- ex.: CNPJ/DAS), lancamentos, regras, contas e historico. Se quiser recriar as
-- categorias de referencia, rode o 099_SeedBase.sql manualmente apos o reset.
--
-- PKs sao UNIQUEIDENTIFIER (Guid): nao ha identity numerica para "resetar para 1".
--
-- IMPORTANTE: manter este arquivo FORA de scripts/sqlserver/ (o DbUp/DbSetup
-- executaria os scripts dessa pasta a cada `dotnet run --project tools/DbSetup`).

-- 1) Desabilita todas as FKs para permitir delecao em qualquer ordem
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- 2) Limpa as tabelas filhas (dados de todos os usuarios, inclusive do admin)
DELETE FROM CategoriaHistorico;
DELETE FROM Despesa;
DELETE FROM Receita;
DELETE FROM RegraDespesa;
DELETE FROM RegraReceita;
DELETE FROM CategoriaDespesa;
DELETE FROM CategoriaReceita;
DELETE FROM ContaBancaria;

-- 3) Remove todos os usuarios, exceto o admin padrao
-- (categorias, contas, lancamentos e regras ja foram apagados acima, inclusive as do admin)
DELETE FROM Usuario WHERE Email <> 'admin@portal.com';

-- 4) Reabilita as FKs
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
