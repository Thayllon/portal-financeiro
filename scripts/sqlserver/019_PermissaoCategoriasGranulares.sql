-- Portal Financeiro - Permissões granulares de categoria (SQL Server, banco local).
-- 019: divide o módulo 'categorias' em 'categorias-receita', 'categorias-despesa'
-- e 'categorias-servico', preservando o nível que cada usuário já tinha.
-- Idempotente: só insere onde falta; remove o módulo legado ao final.

INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), p.UsuarioId, m.Modulo, p.Nivel
FROM PermissaoUsuario p
CROSS JOIN (VALUES ('categorias-receita'), ('categorias-despesa'), ('categorias-servico')) AS m(Modulo)
WHERE p.Modulo = 'categorias'
  AND NOT EXISTS (
      SELECT 1 FROM PermissaoUsuario x
      WHERE x.UsuarioId = p.UsuarioId AND x.Modulo = m.Modulo
  );

-- Garante os 3 módulos (Nenhum) para usuários que nunca tiveram 'categorias'.
INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), u.Id, m.Modulo, 0
FROM Usuario u
CROSS JOIN (VALUES ('categorias-receita'), ('categorias-despesa'), ('categorias-servico')) AS m(Modulo)
WHERE NOT EXISTS (
    SELECT 1 FROM PermissaoUsuario x
    WHERE x.UsuarioId = u.Id AND x.Modulo = m.Modulo
);

-- Remove o módulo legado (ninguém mais o referencia).
DELETE FROM PermissaoUsuario WHERE Modulo = 'categorias';
