-- 100_SeedDemo.sql — SQL Server (paridade com postgres/100_SeedDemo.sql)
-- Idempotente: todos os INSERTs com IF NOT EXISTS
-- Senha de todos: senhasenha | Hash: nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=

-- 1) Usuários
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email='maria@demo.com')
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
VALUES (NEWID(), 'Maria Silva', 'maria@demo.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', 0, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email='joao@demo.com')
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
VALUES (NEWID(), 'João Souza', 'joao@demo.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', 0, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME());

-- Permissoes
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), u.Id, 'parcerias', 1 FROM Usuario u
WHERE u.Email IN ('maria@demo.com','joao@demo.com')
AND NOT EXISTS (SELECT 1 FROM PermissaoUsuario p WHERE p.UsuarioId=u.Id AND p.Modulo='parcerias');

IF NOT EXISTS (SELECT 1 FROM PermissaoUsuario p JOIN Usuario u ON p.UsuarioId=u.Id WHERE u.Email='maria@demo.com' AND p.Modulo='fluxo-adicional-receita')
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), u.Id, 'fluxo-adicional-receita', 1 FROM Usuario u WHERE u.Email='maria@demo.com';

IF NOT EXISTS (SELECT 1 FROM PermissaoUsuario p JOIN Usuario u ON p.UsuarioId=u.Id WHERE u.Email='maria@demo.com' AND p.Modulo='fluxo-adicional-despesa')
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), u.Id, 'fluxo-adicional-despesa', 1 FROM Usuario u WHERE u.Email='maria@demo.com';

-- 2) Contas
IF NOT EXISTS (SELECT 1 FROM ContaBancaria c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Banco Premium')
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Banco Premium', 'Nubank', 2, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';

IF NOT EXISTS (SELECT 1 FROM ContaBancaria c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Carteira')
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Carteira', 'Inter', 1, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';

IF NOT EXISTS (SELECT 1 FROM ContaBancaria c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND c.Nome='Banco Simples')
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Banco Simples', 'Caixa', 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';

IF NOT EXISTS (SELECT 1 FROM ContaBancaria c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='admin@portal.com')
INSERT INTO ContaBancaria (Id, IdUsuario, Nome, Banco, Tipo, EhPadrao, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Conta Principal', 'Banco do Brasil', 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='admin@portal.com';

-- 3) Pessoas
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND p.Nome='Empresa Alpha LTDA')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Empresa Alpha LTDA', '(11) 99999-0001', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND p.Nome='Beta Comércio SA')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Beta Comércio SA', '(11) 99999-0002', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND p.Nome='Carlos Parceiro')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Carlos Parceiro', '(11) 98888-0001', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND p.Nome='Ana Parceira')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Ana Parceira', '(11) 98888-0002', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND p.Nome='Cliente Capenga ME')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Cliente Capenga ME', '(21) 97777-0001', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';
IF NOT EXISTS (SELECT 1 FROM Pessoa p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND p.Nome='Parceiro Local')
INSERT INTO Pessoa (Id, IdUsuario, Nome, Telefone, Tipo, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Parceiro Local', '(21) 97777-0002', 2, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';

-- 4) Parceria Maria
IF NOT EXISTS (SELECT 1 FROM Parceria p JOIN Usuario u ON p.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND p.Nome='Parceria Alpha-Carlos')
INSERT INTO Parceria (Id, IdUsuario, Nome, IdParceiro, IdCliente, Valor, PercentualParceiro, Ativo, DataCadastro, DataAlteracao)
SELECT NEWID(), u.Id, 'Parceria Alpha-Carlos', par.Id, cli.Id, 50000, 15, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
FROM Usuario u JOIN Pessoa cli ON cli.IdUsuario=u.Id AND cli.Nome='Empresa Alpha LTDA' JOIN Pessoa par ON par.IdUsuario=u.Id AND par.Nome='Carlos Parceiro' WHERE u.Email='maria@demo.com';

-- 5) Categorias Maria
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Vendas')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Vendas', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Consultoria')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Consultoria', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Serviços Premium')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Serviços Premium', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Recorrente')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Recorrente', pai.Id, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u JOIN CategoriaReceita pai ON pai.IdUsuario=u.Id AND pai.Nome='Vendas' WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Suporte')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Suporte', pai.Id, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u JOIN CategoriaReceita pai ON pai.IdUsuario=u.Id AND pai.Nome='Consultoria' WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Operacional')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Operacional', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Marketing')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Marketing', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Administrativo')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Administrativo', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Ads')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Ads', pai.Id, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u JOIN CategoriaDespesa pai ON pai.IdUsuario=u.Id AND pai.Nome='Marketing' WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Escritório')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Escritório', pai.Id, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u JOIN CategoriaDespesa pai ON pai.IdUsuario=u.Id AND pai.Nome='Administrativo' WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaServico c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Desenvolvimento')
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Desenvolvimento', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaServico c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Design')
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Design', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaServico c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='maria@demo.com' AND c.Nome='Consultoria Tec')
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Consultoria Tec', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='maria@demo.com';

-- João/Admin categorias
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND c.Nome='Serviços')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Serviços', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND c.Nome='Vendas')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Vendas', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND c.Nome='Custos Fixos')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Custos Fixos', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='joao@demo.com' AND c.Nome='Insumos')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Insumos', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='joao@demo.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaReceita c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='admin@portal.com' AND c.Nome='Vendas')
INSERT INTO CategoriaReceita (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Vendas', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='admin@portal.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaDespesa c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='admin@portal.com' AND c.Nome='Operacional')
INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Operacional', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='admin@portal.com';
IF NOT EXISTS (SELECT 1 FROM CategoriaServico c JOIN Usuario u ON c.IdUsuario=u.Id WHERE u.Email='admin@portal.com' AND c.Nome='Geral')
INSERT INTO CategoriaServico (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao) SELECT NEWID(), u.Id, 'Geral', NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM Usuario u WHERE u.Email='admin@portal.com';

-- Nota: lançamentos 2024-2026 para SQL Server podem ser inseridos manualmente ou via ferramenta;
-- este script cria a base estática. Bulk de receitas/despesas idêntico ao Postgres pode ser
-- executado via aplicacao ou import. Para demo completa em SQL Server, rode também o script Postgres
-- adaptado ou use a API para criar lançamentos. O seed mínimo acima já deixa Maria/João navegáveis;
-- o volume mensal completo é gerado automaticamente no Postgres (prod Neon).
GO
