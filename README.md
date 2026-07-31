# Portal Financeiro

Sistema de controle financeiro pessoal para gerenciar receitas e despesas recorrentes, com acompanhamento mensal e dashboard.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 11 + ASP.NET Core |
| Frontend | Angular 22 (standalone, Signals) |
| Banco | SQL Server LocalDB |
| ORM | Dapper |
| Auth | JWT Bearer |
| Migrations | DbUp |
| Ícones | Lucide Angular |

## Estrutura

```
portal-financeiro/
├── src/
│   ├── PortalFinanceiro.API/        # Controllers, middleware, Program.cs
│   ├── PortalFinanceiro.Core/       # Domain + Application (Clean Architecture)
│   └── PortalFinanceiro.Web/        # Angular 22
├── scripts/sql/                     # Migrations DbUp
├── tools/DbSetup/                   # Ferramenta para rodar migrations
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

## Funcionalidades

- Dashboard com visão mensal, previsão de 3 meses e resumo por conta bancária
- Cadastro de receitas e despesas recorrentes (data início + data fim → geração automática de parcelas mensais)
- Aba Mensal para marcar recebida/paga (como um "caderno digital")
- Contas bancárias (PF/PJ) e categorias de receita/despesa
- Autenticação JWT
- Preview de parcelas ao cadastrar uma recorrente

## Fluxo de desenvolvimento

- `master` — versão estável, atualizada apenas sob autorização
- `develop` — branch de trabalho ativa
