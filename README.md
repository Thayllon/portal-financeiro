# Portal Financeiro

Sistema de controle financeiro pessoal (PF/PJ) que reflete o extrato real de todas as contas, com lanÃ§amentos recorrentes/avulsos e categorias compartilhadas com auditoria.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 11 + ASP.NET Core (Clean Architecture, Dapper + Polly) |
| Frontend | Angular 22 (standalone, Signals) |
| Banco | PostgreSQL (local `localhost:5432/portal_financeiro` e produção Neon/Oracle Cloud) |
| Auth | JWT Bearer |
| Migrations | Scripts SQL em `scripts/postgres/` (aplicados manualmente) |
| Ícones | Lucide Angular |
| Deploy | Docker Compose — local e produção (PostgreSQL) |

## Quick Start

```bash
# Banco PostgreSQL local (replica em D:\projetos\postgres-replica, auto-start no logon)
# Schema + dados: aplicar scripts/postgres/001..106 em banco novo ou sincronizar via sincronizar-banco.ps1

# Backend (http://localhost:5178)
dotnet build PortalFinanceiro.API.slnx
dotnet run --project src/PortalFinanceiro.API

# Frontend (http://localhost:4200)
cd src/PortalFinanceiro.Web && npm install && npm start

# Testes
dotnet test PortalFinanceiro.API.slnx      # backend (xUnit + FluentAssertions)
cd src/PortalFinanceiro.Web && npm test   # frontend (Vitest)
```

> O repositÃ³rio nÃ£o faz seed de dados nem de usuÃ¡rio admin: o primeiro usuÃ¡rio
> admin Ã© criado pela tela de cadastro/login inicial (ou manualmente no banco).

## DocumentaÃ§Ã£o

A documentaÃ§Ã£o completa por Ã¡rea estÃ¡ em [`doc/`](doc/README.md):
[`back.md`](doc/back.md) (API), [`front.md`](doc/front.md) (Angular), [`banco.md`](doc/banco.md) (scripts/schema), [`deploy-local.md`](doc/deploy-local.md) (Docker local), [`infra.md`](doc/infra.md) (Oracle Cloud) e [`primeiros-passos.md`](doc/primeiros-passos.md) (do zero).

> Guia para agentes de IA: [`AGENTS.md`](AGENTS.md).

## Deploy

| Ambiente | Comando | Acesso |
|----------|---------|--------|
| **Local (tudo em um link)** | `Copy-Item .env.example .env` + `docker compose -f docker-compose.local.yml up -d --build` | `http://localhost:8080` |
| **ProduÃ§Ã£o Oracle Cloud (R$ 0)** | Ver [doc/infra.md](doc/infra.md) | URL/IP da VM |
