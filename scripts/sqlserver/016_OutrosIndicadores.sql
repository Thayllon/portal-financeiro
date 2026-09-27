-- Portal Financeiro - Permissão especial "Outros indicadores" (SQL Server, banco local).
-- 016: libera os tiles de Contratos/Parcerias do dashboard para quem já usa
-- fluxo adicional (receita ou despesa). Não toca Postgres (scripts/postgres/ mantido como está).

INSERT INTO PermissaoUsuario (Id, UsuarioId, Modulo, Nivel)
SELECT NEWID(), U.Id, 'outros-indicadores', 1
FROM Usuario U
WHERE NOT EXISTS (
    SELECT 1 FROM PermissaoUsuario P
    WHERE P.UsuarioId = U.Id AND P.Modulo = 'outros-indicadores'
)
AND (
    U.IsAdmin = 1
    OR EXISTS (
        SELECT 1 FROM PermissaoUsuario F
        WHERE F.UsuarioId = U.Id
          AND F.Modulo IN ('fluxo-adicional-receita', 'fluxo-adicional-despesa')
          AND F.Nivel >= 1
    )
);
