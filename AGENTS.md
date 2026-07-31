# Portal Financeiro — Guia para Agentes

## Comandos

```bash
# Setup do banco (SQL Server LocalDB)
dotnet run --project tools/DbSetup

# Build backend
dotnet build PortalFinanceiro.API.slnx

# Rodar API (http://localhost:5178)
dotnet run --project src/PortalFinanceiro.API

# Rodar frontend (http://localhost:4200)
cd src/PortalFinanceiro.Web && npm start

# Build frontend
cd src/PortalFinanceiro.Web && npm run build

# Testes frontend
cd src/PortalFinanceiro.Web && npm test
```

## Login padrão (ambiente dev)

| Campo  | Valor               |
|--------|---------------------|
| E-mail | `admin@portal.com`  |
| Senha  | `123456`            |

## Arquitetura

.NET 11 + Angular 22. Clean Architecture com 3 projetos backend + 1 frontend.

### Backend

| Projeto | Função |
|---------|--------|
| `PortalFinanceiro.API` | Controllers, middleware, DI, startup |
| `PortalFinanceiro.Core` | Domain entities, DTOs, services, interfaces |
| `PortalFinanceiro.Infrastructure` | Dapper repositories, IoC |

### Frontend (Angular 22)

- **Standalone components** com Angular signals
- **Design system** em `src/app/design-system/styles/`
- **Ícones**: Lucide Angular (`@lucide/angular`)
- **Componentes compartilhados** em `src/app/shared/components/`
- **Features** em `src/app/features/` (dashboard, receitas, despesas, contas, categorias)

## Padrões de Código

- Usar `signal()` para estado reativo, `computed()` para derivados
- Componentes standalone com `imports` explícitos
- Lucide icons: `<svg lucideIcon="nome" [size]="16" />`
- Forms: ControlValueAccessor para componentes reutilizáveis (CustomSelect)
- SCSS: mixins do design system (`_responsive.scss`, `_transitions.scss`, etc.)
- Nunca hardcodar hex fora do design system
- Responsividade: breakpoint mobile em 767px

## Estrutura de Pastas

```
src/app/
├── core/
│   ├── layout/          # LayoutComponent (sidebar + content)
│   ├── models/          # Interfaces de domínio
│   ├── repositories/    # Services HTTP
│   ├── services/        # AuthService, NotificationService
│   └── guards/          # authGuard
├── design-system/
│   └── styles/          # Tokens, mixins, variáveis CSS
├── features/
│   ├── dashboard/
│   ├── receitas/
│   ├── despesas/
│   ├── contas/
│   ├── categorias-receita/
│   └── login/
└── shared/
    ├── components/      # Componentes reutilizáveis
    ├── composables/     # useListPagination
    ├── constants/       # PAGE_SIZE_OPTIONS
    ├── pipes/           # CurrencyBRLPipe
    └── services/        # ConfirmService
```

## Fluxo de Desenvolvimento

- `master` — versão estável
- `develop` — branch de trabalho ativa
