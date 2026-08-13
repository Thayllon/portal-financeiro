-- Seed base (executado por último): usuário administrador padrão (admin@portal.com / senhasenha)
-- e categorias fiscais compartilhadas CNPJ -> DAS.
-- Idempotente: executa somente o que ainda não existe.
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com');

INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'CNPJ', NULL, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM Usuario u
WHERE u.Email = 'admin@portal.com'
  AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa WHERE Nome = 'CNPJ' AND IdUsuario = u.Id AND CategoriaPaiId IS NULL);

INSERT INTO CategoriaDespesa (Id, IdUsuario, Nome, CategoriaPaiId, Ativo, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), u.Id, 'DAS', cnpj.Id, TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM Usuario u
JOIN CategoriaDespesa cnpj ON cnpj.IdUsuario = u.Id AND cnpj.Nome = 'CNPJ' AND cnpj.CategoriaPaiId IS NULL
WHERE u.Email = 'admin@portal.com'
  AND NOT EXISTS (SELECT 1 FROM CategoriaDespesa WHERE Nome = 'DAS' AND IdUsuario = u.Id AND CategoriaPaiId = cnpj.Id);