# Portal Financeiro

Sistema de controle financeiro pessoal que **reflete o extrato real de todas as contas** (PF/PJ). Uma tela por tipo, com lançamentos recorrentes e avulsos.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 11 + ASP.NET Core |
| Frontend | Angular 22 (standalone, Signals) |
| Banco | SQL Server LocalDB |
| ORM | Dapper + Polly retry |
| Auth | JWT Bearer |
| Migrations | DbUp |
| Ícones | Lucide Angular |

## Estrutura

```
portal-financeiro/
├── src/
│   ├── PortalFinanceiro.API/          # Controllers, middleware, Program.cs
│   ├── PortalFinanceiro.Core/         # Domain + Application (Clean Architecture)
│   └── PortalFinanceiro.Web/          # Angular 22
├── scripts/sql/                       # Migrations DbUp
├── tools/DbSetup/                     # Ferramenta para rodar migrations
└── test/
```

## Como rodar

### 1. Criar banco de dados

```bash
dotnet run --project tools/DbSetup
```

> Cria o banco `PortalFinanceiro` no SQL Server LocalDB e executa as migrations.

### 2. Iniciar API

```bash
dotnet run --project src/PortalFinanceiro.API
```

> A API roda em `http://localhost:5178`  
> Swagger disponível em `http://localhost:5178/swagger`

### 3. Iniciar frontend

```bash
cd src/PortalFinanceiro.Web
npm install
ng serve
```

> O Angular roda em `http://localhost:4200`

## Login padrão (ambiente dev)

| Campo | Valor |
|-------|-------|
| Email | `admin@portal.com` |
| Senha | `123456` |

> O usuário admin é criado automaticamente na primeira inicialização da API.

## Modelo de dados

### Conceitos

| Conceito | Descrição |
|----------|-----------|
| **Conta** | Nubank PF, Itaú PJ — onde o dinheiro entra/sai |
| **Categoria + Subcategoria** | Classificação (ex: Lazer → pizza, lanche) |
| **Receita/Despesa** | Lançamento único no mês (data real do gasto) |
| **Regra recorrente** | Comportamento "repete" — gera parcelas mensais automáticas |
| **Avulsa** | Lançamento manual, sem recorrência |

### Regra de recorrência

- **Fixo** (salário, internet): valor sempre igual → parcelas com valor fixo
- **Variável** (água, luz): lançamento avulso no mês quando a conta chega

### Fluxo

1. Cadastra contas (PF/PJ) e categorias (+subcategorias)
2. Cadastra receitas/despesas com "Repete?" → gera parcelas automáticas
3. Lança avulsas no dia do gasto (pizza, corte de cabelo)
4. Marca recebido/pago conforme vai pagando
5. Dashboard mostra resumo por conta e categoria

## Rotas da API

| Rota | Método | Descrição |
|------|--------|-----------|
| `/api/auth/login` | POST | Login |
| `/api/receitas` | GET/POST/PUT/DELETE | Lançamentos de receita |
| `/api/receitas/{id}/receber` | POST | Marcar como recebido |
| `/api/receitas/{id}/estornar` | POST | Estornar recebimento |
| `/api/despesas` | GET/POST/PUT/DELETE | Lançamentos de despesa |
| `/api/despesas/{id}/pagar` | POST | Marcar como pago |
| `/api/despesas/{id}/estornar` | POST | Estornar pagamento |
| `/api/regras-receitas` | GET/PUT/DELETE | Regras recorrentes de receita |
| `/api/regras-despesas` | GET/PUT/DELETE | Regras recorrentes de despesa |
| `/api/contas-bancarias` | GET/POST/PUT/DELETE | Contas bancárias |
| `/api/categorias/receita` | GET/POST/PUT/DELETE | Categorias de receita |
| `/api/categorias/despesa` | GET/POST/PUT/DELETE | Categorias de despesa |
| `/api/usuarios` | GET/POST/PUT · PATCH /{id}/ativo | Gerenciamento de usuários (somente admin) |
| `/api/dashboard` | GET | Dashboard com resumo |

## Fluxo de desenvolvimento

- `main` — versão estável, atualizada apenas sob autorização
- `develop` — branch de trabalho ativa
