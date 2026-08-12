# Portal Financeiro

Sistema de controle financeiro pessoal (PF/PJ) com lançamentos recorrentes e avulsos, categorias compartilhadas com auditoria e encargos automáticos (DAS e INSS).

## Rápido

```bash
# Banco novo (SQL Server LocalDB) — cria DB + schema + admin + categorias fiscais
dotnet run --project tools/DbSetup

# API (http://localhost:5178)
dotnet run --project src/PortalFinanceiro.API

# Frontend (http://localhost:4200)
cd src/PortalFinanceiro.Web && ng serve
```

**Login dev:** `admin@portal.com` / `senhasenha`

## Documentação

> Documentação por área em [`doc/`](doc/README.md): [`back.md`](doc/back.md) (API), [`front.md`](doc/front.md) (Angular), [`banco.md`](doc/banco.md) (scripts/schema), [`infra.md`](doc/infra.md) (placeholder) e [`primeiros-passos.md`](doc/primeiros-passos.md) (como rodar do zero).
