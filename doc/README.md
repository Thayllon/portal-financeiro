# Portal Financeiro — Documentação

Sistema de controle financeiro pessoal que **reflete o extrato real de todas as contas** (PF/PJ). Uma tela por tipo, com lançamentos recorrentes e avulsos, categorias compartilhadas com auditoria e encargos automáticos (DAS e INSS).

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 11 + ASP.NET Core |
| Frontend | Angular 22 (standalone, Signals) |
| Banco | SQL Server LocalDB (scripts Postgres também mantidos) |
| ORM | Dapper + Polly retry |
| Auth | JWT Bearer |
| Migrations | DbUp |
| Ícones | Lucide Angular |

## Índice da documentação

| Documento | O que contém | Quando consultar |
|-----------|--------------|------------------|
| [back.md](back.md) | Backend: arquitetura, projetos, rotas da API, como rodar/buildar, padrões | Mexer na API/Core/Infra; descobrir endpoint |
| [front.md](front.md) | Frontend: estrutura, features, como rodar/buildar/testar, padrões de UI | Mexer no Angular; criar tela/componente |
| [banco.md](banco.md) | Banco: scripts por provider, DbSetup, modelo de dados, encargos, seed | Mexer em migração/schema; entender tabelas |
| [infra.md](infra.md) | Infraestrutura (placeholder — ainda vamos montar) | Subir em produção/conteinerizar |
| [primeiros-passos.md](primeiros-passos.md) | Como rodar do zero na primeira vez (banco → API → front) | Ambiente novo; perder tudo e recomeçar |

## Estrutura do repositório

```
portal-financeiro/
├── src/
│   ├── PortalFinanceiro.API/          # Controllers, middleware, Program.cs
│   ├── PortalFinanceiro.Core/         # Domain + Application (Clean Architecture)
│   ├── PortalFinanceiro.Infrastructure# Dapper repositories, IoC
│   └── PortalFinanceiro.Web/          # Angular 22
├── scripts/
│   ├── sqlserver/                     # Migrations DbUp (SQL Server) — from scratch
│   │   ├── 001_CriarTabelas.sql       #   schema unificado completo
│   │   └── 099_SeedBase.sql           #   admin + categorias CNPJ → DAS/INSS
│   └── postgres/                      # Mesmo conjunto para PostgreSQL
├── doc/                               # Documentação (este índice + arquivos por área)
├── tools/DbSetup/                     # Ferramenta para rodar migrations
└── test/
```

## Conceitos gerais

| Conceito | Descrição |
|----------|-----------|
| **Conta** | Nubank PF, Itaú PJ — onde o dinheiro entra/sai |
| **Categoria + Subcategoria** | Classificação (ex: CNPJ → DAS, INSS). **Compartilhadas entre todos os usuários**; editar/excluir só o dono ou admin |
| **Receita/Despesa** | Lançamento único no mês (data real do gasto) |
| **Regra recorrente** | Comportamento "repete" — gera parcelas mensais automáticas |
| **Avulsa** | Lançamento manual, sem recorrência |
| **DAS** | Encargo sobre receita com "nota fiscal" (avulsa ou parcela recorrente) — gera despesa DAS automática |
| **Pró-labore + INSS** | Cadastro mensal de pró-labore que gera despesa INSS automática |
| **Auditoria de categorias** | `CategoriaHistorico` registra criado/editado/excluído de categorias |

### Fluxo

1. Cadastra contas (PF/PJ) e categorias (+subcategorias)
2. Cadastra receitas/despesas com "Repete?" → gera parcelas automáticas
3. Lança avulsas no dia do gasto (pizza, corte de cabelo)
4. Receitas com nota fiscal podem gerar **DAS** automático (avulsa ou parcela)
5. Cadastra **pró-labore** mensal → gera **INSS** automático
6. Marca recebido/pago conforme vai pagando
7. Dashboard mostra resumo por conta e categoria

## Login padrão (ambiente dev)

| Campo | Valor |
|-------|-------|
| Email | `admin@portal.com` |
| Senha | `senhasenha` |

> O usuário admin e as categorias fiscais são criados pelo seed (`099_SeedBase.sql`).
> Em banco já existente sem o seed, a API cria o admin automaticamente na primeira
> inicialização.

## Fluxo de desenvolvimento

- `main` — versão estável, atualizada apenas sob autorização
- `develop` — branch de trabalho ativa
- Commits locais (sem push) agrupados por área: `banco` (scripts), `back` (API/Core/Infra), `front` (Web), `doc` (documentação)
