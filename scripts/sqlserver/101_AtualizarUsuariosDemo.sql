-- 101_AtualizarUsuariosDemo.sql — SQL Server (paridade com postgres/101_AtualizarUsuariosDemo.sql)
-- Atualiza e-mail e senha dos usuários demo (Maria e João) para ambiente de produção.
-- Novas credenciais: maria@portal.com / 123456 e joao@portal.com / 123456
-- Idempotente: pode ser executado mais de uma vez sem erro.
-- Executar APÓS 099_SeedBase.sql e 100_SeedDemo.sql.

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- Hash PBKDF2 (SHA256, 100.000 iterações, salt 16 bytes, hash 32 bytes) de "123456"
-- Formato {base64(salt)}.{base64(hash)} — mesmo do PasswordService.
-- Aplica e-mail novo apenas se o antigo existir e o novo ainda não.
UPDATE Usuario
SET Email = 'maria@portal.com',
    DataAlteracao = SYSUTCDATETIME()
WHERE Email = 'maria@demo.com'
  AND NOT EXISTS (SELECT 1 FROM Usuario u2 WHERE u2.Email = 'maria@portal.com');

UPDATE Usuario
SET Email = 'joao@portal.com',
    DataAlteracao = SYSUTCDATETIME()
WHERE Email = 'joao@demo.com'
  AND NOT EXISTS (SELECT 1 FROM Usuario u2 WHERE u2.Email = 'joao@portal.com');

-- Garante a senha nova e libera o primeiro acesso, independentemente do e-mail já ter sido trocado.
UPDATE Usuario
SET SenhaHash = '9pY0pNVbjoJmULBS8GtvbA==.RwotAp0SRR2PFT3gFykFHlgmVqA699cndfDhr+r5X98=',
    PrimeiroAcesso = 0,
    DataAlteracao = SYSUTCDATETIME()
WHERE Email IN ('maria@portal.com', 'joao@portal.com');
