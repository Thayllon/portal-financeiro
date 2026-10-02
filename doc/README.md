# Portal Financeiro — Documentação

Sistema de controle financeiro pessoal que **reflete o extrato real de todas as contas** (PF/PJ). Uma tela por tipo, com lançamentos recorrentes e avulsos e categorias compartilhadas com auditoria.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 11 + ASP.NET Core |
| Frontend | Angular 22 (standalone, Signals) |
| Banco | PostgreSQL (local `localhost:5432/portal_financeiro` e produção Neon/Oracle Cloud) |
| ORM | Dapper + Polly retry |
| Auth | JWT Bearer |
| Migrations | Scripts SQL em `scripts/postgres/` (aplicados manualmente) |
| Ícones | Lucide Angular |
| Deploy | Docker Compose — local e produção (PostgreSQL) + Render (API) + Vercel (Web) |

## Índice da documentação

| Documento | O que contém | Quando consultar |
|-----------|--------------|------------------|
| [regras.md](regras.md) | **Wiki micro das regras de negócio**: fonte única das 6 regras críticas, cenários `C<regra>.<n>` e débitos técnicos | Antes de mexer em regra; a tela `/testes` executa estes cenários ao vivo |
| [back.md](back.md) | Backend: arquitetura, projetos, rotas da API, como rodar/buildar, padrões | Mexer na API/Core/Infra; descobrir endpoint |
| [front.md](front.md) | Frontend: estrutura, features, como rodar/buildar/testar, padrões de UI | Mexer no Angular; criar tela/componente |
| [banco.md](banco.md) | Banco: scripts PostgreSQL, modelo de dados, sincronização local→Neon | Mexer em migração/schema; entender tabelas |
| [deploy-local.md](deploy-local.md) | **Subir TUDO local com Docker em um link** (`http://localhost:8080`) — PostgreSQL + API + Front | Validar o sistema de ponta a ponta sem instalar nada |
| [infra.md](infra.md) | Deploy **Oracle Cloud Always Free** (R$ 0) — PostgreSQL + API + Front numa VM só | Subir em produção / nuvem |
| [primeiros-passos.md](primeiros-passos.md) | Como rodar do zero na primeira vez (banco → API → front), sem Docker | Ambiente novo; perder tudo e recomeçar |

## Estrutura do repositório

```
portal-financeiro/
├── src/
│   ├── PortalFinanceiro.API/          # Controllers, middleware, Program.cs
│   ├── PortalFinanceiro.Core/         # Domain + Application (Clean Architecture)
│   ├── PortalFinanceiro.Infrastructure# Dapper repositories, IoC
│   └── PortalFinanceiro.Web/          # Angular 22 (+ Dockerfile e nginx.conf)
├── scripts/
│   └── postgres/                      # Schema + incrementais idempotentes (001..106; uso manual)
├── doc/                               # Documentação (este índice + arquivos por área)
├── test/
├── Dockerfile                         # Backend .NET 11 (multi-stage, porta 8080)
├── docker-compose.yml                 # Produção: PostgreSQL + API + Web
├── docker-compose.local.yml           # Local: PostgreSQL + API + Web (link único)
├── .env.example                       # Modelo de variáveis (sem segredos reais)
├── sincronizar-banco.ps1              # Sync PostgreSQL local → Neon (pg_dump)
└── .github/workflows/ci.yml           # CI: build + teste (backend e frontend)
```

## Conceitos gerais

| Conceito | Descrição |
|----------|-----------|
| **Conta** | Nubank PF, Itaú PJ — onde o dinheiro entra/sai |
| **Categoria + Subcategoria** | Classificação (ex: CNPJ → DAS, Lazer → Pizza). **Compartilhadas entre todos os usuários**; editar/excluir só o dono ou admin |
| **Receita/Despesa** | Lançamento único no mês (data real do gasto) |
| **Regra recorrente** | Comportamento "repete" — gera parcelas mensais automáticas |
| **Avulsa** | Lançamento manual, sem recorrência |
| **Auditoria de categorias** | `CategoriaHistorico` registra criado/editado/excluído de categorias |
| **Auditoria de ator** | `CriadoPor`/`AlteradoPor` nas tabelas + log estruturado (`Auditoria`/`AuditoriaNegada` + `X-Correlation-Id`) |

### Fluxo

1. Cadastra contas (PF/PJ) e categorias (+subcategorias)
2. Cadastra receitas/despesas com "Repete?" → gera parcelas automáticas
3. Lança avulsas no dia do gasto (pizza, corte de cabelo)
4. Marca recebido/pago conforme vai pagando
5. Dashboard mostra resumo por conta e categoria

## Login (ambiente dev)

> O repositório **não faz seed de dados nem de admin**. O primeiro usuário é criado
> pelo fluxo de cadastro/login inicial da aplicação (ou manualmente no banco
> usando a mesma hash do `PasswordService`).

## Fluxo de desenvolvimento

- `main` — versão estável, atualizada apenas sob autorização
- `develop` — branch de trabalho ativa
- Commits locais (sem push) agrupados por área: `banco` (scripts), `back` (API/Core/Infra), `front` (Web), `doc` (documentação)
