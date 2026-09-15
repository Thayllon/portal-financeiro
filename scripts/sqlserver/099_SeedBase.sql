-- Seed base: usuário administrador padrão (admin@portal.com / senhasenha).
-- Idempotente: executa somente se não existir.
IF NOT EXISTS (SELECT 1 FROM Usuario WHERE Email = 'admin@portal.com')
BEGIN
    INSERT INTO Usuario (Id, Nome, Email, SenhaHash, IsAdmin, Ativo, PrimeiroAcesso, DataCadastro, DataAlteracao)
    VALUES (NEWID(), 'Admin', 'admin@portal.com', 'nc0RKfw9YhrKHokj4xZ3AQ==.11eIHgy/7VkSsZ734otOeP/9387OU5Ka6HtuZBumDJY=', 1, 1, 0, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

-- Garante o módulo 'parcerias' para usuários que ainda não o possuem (inclui o admin do seed).
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), Id, 'parcerias', 0
FROM Usuario
WHERE Id NOT IN (
    SELECT UsuarioId FROM PermissaoUsuario WHERE Modulo = 'parcerias'
);
