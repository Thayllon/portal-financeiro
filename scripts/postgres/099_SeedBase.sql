-- Seed base: usuário administrador padrão (admin@portal.com / senhasenha).
-- Idempotente: executa somente se não existir.
INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
SELECT gen_random_uuid(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', TRUE, TRUE, FALSE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com');

-- Garante o módulo 'parcerias' para usuários que ainda não o possuem (inclui o admin do seed).
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT gen_random_uuid(), Id, 'parcerias', 0
FROM Usuario
WHERE Id NOT IN (
    SELECT UsuarioId FROM PermissaoUsuario WHERE Modulo = 'parcerias'
);