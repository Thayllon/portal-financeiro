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

> Documentação por área em [`doc/`](doc/README.md): [`back.md`](doc/back.md) (API), [`front.md`](doc/front.md) (Angular), [`banco.md`](doc/banco.md) (scripts/schema), [`deploy-local.md`](doc/deploy-local.md) (rodar tudo local com Docker em um link), [`infra.md`](doc/infra.md) (deploy Oracle Cloud) e [`primeiros-passos.md`](doc/primeiros-passos.md) (como rodar do zero).

## Deploy

| Ambiente | Comando | Acesso |
|----------|---------|--------|
| **Local (tudo em um link)** | `Copy-Item .env.example .env` + `docker compose -f docker-compose.local.yml up -d --build` | `http://localhost:8080` |
| **Produção Oracle Cloud (R$ 0)** | Ver [doc/infra.md](doc/infra.md) | URL/IP da VM |
