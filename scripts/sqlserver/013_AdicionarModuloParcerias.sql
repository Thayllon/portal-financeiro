-- Migration: Adicionar módulo 'parcerias' aos usuários existentes
-- Idempotente: pode ser re-executado com segurança

-- Inserir módulo 'parcerias' com nível 0 (Nenhum) para todos os usuários que ainda não possuem
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), Id, 'parcerias', 0
FROM Usuario
WHERE Id NOT IN (
    SELECT UsuarioId FROM PermissaoUsuario WHERE Modulo = 'parcerias'
);
