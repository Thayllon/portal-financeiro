# Frontend — Portal Financeiro

## Stack

- **Angular 22** — componentes **standalone** com **Signals**
- **Design system** próprio em `src/app/design-system/styles/` (tokens, mixins, variáveis)
- **Ícones**: Lucide Angular (`@lucide/angular`)
- **Componentes reutilizáveis** em `src/app/shared/components/`
- **Features** em `src/app/features/` (dashboard, receitas, despesas, contas, categorias, clientes, parceiros, usuarios)

## Como rodar / buildar / testar

```bash
# Instalar dependências (primeira vez)
npm install

# Rodar (http://localhost:4200)
npm start        # = ng serve

# Build de produção
npm run build

# Testes unitários (headless)
npm test
```

> O frontend consome a API em `http://localhost:5178` — suba o backend antes.
> Ver [primeiros-passos.md](primeiros-passos.md).

## Estrutura de pastas

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
    ├── components/      # Componentes reutilizáveis (LancamentoModal, CustomSelect...)
    ├── composables/     # useListPagination
    ├── constants/       # PAGE_SIZE_OPTIONS
    ├── pipes/           # CurrencyBRLPipe
    └── services/        # ConfirmService
```

## Padrões de código

Regras completas no [AGENTS.md](../AGENTS.md). Resumo:

- **Signals**: `signal()` para estado, `computed()` para derivados
- Componentes standalone com `imports` explícitos
- Ícones Lucide: `<svg lucideIcon="nome" [size]="16" />`
- Forms: `ControlValueAccessor` para componentes reutilizáveis (CustomSelect)
- SCSS com mixins do design system (`_responsive.scss`, `_transitions.scss`, etc.)
- Nunca enviar `undefined` como query param — usar spread condicional
- Status enviado como `number` (1 ou 2), nunca string
- Repositórios NÃO enviam `idUsuario` nos params (vem do JWT)

## Design System

- Nunca hardcodar hex fora do design system
- Breakpoint mobile: 767px
- Transições com `will-change` para GPU acceleration
- Tokens: `_tokens.scss`, `_colors.scss`, `_responsive.scss`, `_transitions.scss`

## Telas / rotas

| Rota | Feature | Descrição |
|------|---------|-----------|
| `/login` | login | Autenticação |
| `/dashboard` | dashboard | Resumo por conta e categoria |
| `/receitas` | receitas | Lançamentos de receita (avulsas e recorrentes) |
| `/despesas` | despesas | Lançamentos de despesa |
| `/contas` | contas | Contas bancárias |
| `/categorias` | categorias-receita | Categorias compartilhadas (com subcategorias) |
| `/clientes` | clientes | Cadastro de clientes (tipo Cliente) |
| `/parceiros` | parceiros | Cadastro de parceiros (tipo Parceiro) |
| `/usuarios` | usuarios | Usuários e permissões (admin) |

### Menu lateral

- **Dashboard**, **Receitas** e **Despesas** ficam no nível principal.
- **Cadastro Básico** é um grupo colapsável que reúne, nesta ordem: **Contas**, **Categorias**, **Cliente**, **Parceiro** e **Usuários** (admin).
- O ícone `user-key` fica reservado para quando o item **Permissões** voltar.
