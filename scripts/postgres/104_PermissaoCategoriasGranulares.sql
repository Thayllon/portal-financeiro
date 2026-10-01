-- 104_PermissaoCategoriasGranulares.sql — PostgreSQL (Neon)
-- Divide o módulo 'categorias' em 'categorias-receita', 'categorias-despesa'
-- e 'categorias-servico', preservando o nível que cada usuário já tinha.
-- IDEMPOTENTE. Execute no Neon (SQL Editor ou psql) após atualizar o código.

INSERT INTO permissaousuario (id, usuarioid, modulo, nivel)
SELECT gen_random_uuid(), p.usuarioid, m.modulo, p.nivel
FROM permissaousuario p
CROSS JOIN (VALUES ('categorias-receita'), ('categorias-despesa'), ('categorias-servico')) AS m(modulo)
WHERE p.modulo = 'categorias'
  AND NOT EXISTS (
      SELECT 1 FROM permissaousuario x
      WHERE x.usuarioid = p.usuarioid AND x.modulo = m.modulo
  );

-- Garante os 3 módulos (Nenhum) para usuários que nunca tiveram 'categorias'.
INSERT INTO permissaousuario (id, usuarioid, modulo, nivel)
SELECT gen_random_uuid(), u.id, m.modulo, 0
FROM usuario u
CROSS JOIN (VALUES ('categorias-receita'), ('categorias-despesa'), ('categorias-servico')) AS m(modulo)
WHERE NOT EXISTS (
    SELECT 1 FROM permissaousuario x
    WHERE x.usuarioid = u.id AND x.modulo = m.modulo
);

-- Remove o módulo legado (ninguém mais o referencia).
DELETE FROM permissaousuario WHERE modulo = 'categorias';
