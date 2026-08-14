-- Portal Financeiro - Reset de banco de desenvolvimento (SQL Server)
-- Mantem apenas o usuario padrao admin@portal.com (e seus dados de referencia).
-- Todos os demais usuarios e lancamentos (receitas, despesas, regras, contas,
-- categorias, historico) sao removidos.
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
DELETE FROM Usuario WHERE Email <> 'admin@portal.com';

-- (Opcional) Se quiser remover tambem as categorias de referencia do admin
-- (CNPJ/DAS), descomente. Elas NAO voltam sozinhas apos o DbUp (o SeedBase
-- ja foi aplicado/trackado). Para restaura-las, rode o 099_SeedBase.sql manualmente.
-- DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT Id FROM Usuario WHERE Email = 'admin@portal.com');
-- DELETE FROM CategoriaDespesa WHERE IdUsuario = @AdminId;

-- 4) Reabilita as FKs
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
