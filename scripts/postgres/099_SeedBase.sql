-- Seed base: usuário administrador padrão (admin@portal.com / senhasenha).
-- Idempotente: executa somente se não existir.
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com');