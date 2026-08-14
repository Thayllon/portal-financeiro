# Portal Financeiro — Guia para Agentes

## Fluxo de Trabalho (obrigatório)

Para CADA interação/alteração no projeto:

1. **Criar branch a partir de `develop`** antes de qualquer mudança:
   `git checkout -b <nome-da-branch> develop` (ex.: `feature/`, `fix/`, `docs/`).
2. Fazer o trabalho inteiramente na branch.
3. **Ao final, perguntar ao usuário**:
   - se deve **commitar** as mudanças; e
   - se deve **fazer merge da branch na `develop`**.
4. Nunca commitar nem fazer merge sem confirmação explícita do usuário.

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
| Senha  | `senhasenha`        |

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

### Scripts e Documentação

- **Scripts de banco**: `scripts/sqlserver/` (SQL Server, from scratch) e `scripts/postgres/`
- **Documentação**: em `doc/` — `README.md` é o índice; arquivos por área (`back.md`, `front.md`, `banco.md`, `infra.md`, `primeiros-passos.md`). Manter sempre atualizada.

## Padrões de Código

### Backend (C#)

- **Entidades**: Usar `private set` em TODAS as propriedades de domínio
- **Propriedades de navegação** (string display): usar DTO/projeção separada, NÃO `public set` na entidade
- **Result pattern**: Services retornam `Result<T>` em vez de exceptions
- **FluentValidation**: Validators complementam entidades, NÃO substituem
- **Status enum**: Usar `StatusMensal` (1=Pendente, 2=Realizado), NÃO string
- **Controllers**: Extrair `idUsuario` de `User.FindFirst(ClaimTypes.NameIdentifier)`, NUNCA de query params
- **Controllers**: Apenas chamar service + ApiResponse, NUNCA conter lógica de negócio
- **Services**: Orquestrar operações, delegar regras de domínio para entidades
- **LancamentoHelper**: Fica em `Domain/Services/`, é lógica de domínio
- **Categorias compartilhadas**: editar/excluir só dono (`IdUsuario`) ou admin — senão `Erro.Permissao` (HTTP 403)
- **Auditoria de categorias**: toda mutação (criar/editar/excluir, incl. subcategorias) grava `CategoriaHistorico` via `ICategoriaHistoricoRepository`
- **Leitura com projeção**: usar `*Projecao` (ex.: `ReceitaProjecao`, `DespesaProjecao`) em `Domain/Projections/` para nomes display; entidades com `private set`
- **Mutações**: re-buscar projeção via `ObterProjecaoPorIdAsync` após persistir entidade
- **Fluxo recorrente**: usar `TransactionScope` para atomicidade entre regra + parcelas

### Frontend (TypeScript)

- **Signals**: Usar `signal()` para estado, `computed()` para derivados
- **Componentes standalone** com `imports` explícitos
- **Lucide icons**: `<svg lucideIcon="nome" [size]="16" />`
- **Forms**: ControlValueAccessor para componentes reutilizáveis (CustomSelect)
- **SCSS**: Mixins do design system (`_responsive.scss`, `_transitions.scss`, etc.)
- **Params**: NUNCA enviar `undefined` como query param — usar spread condicional
- **Status**: Enviar como `number` (1 ou 2), NÃO como string
- **Repositorios**: NÃO incluir `idUsuario` nos params (vem do JWT)
- **Categoria/Regra repositories**: usar classe base com parâmetro `rota` para evitar duplicação
- **LancamentoRepository**: usar `LancamentoFiltros` compartilhado (alias `ReceitaFiltros`/`DespesaFiltros`)

### Design System

- Nunca hardcodar hex fora do design system
- Responsividade: breakpoint mobile em 767px
- Transições: usar `will-change` para GPU acceleration
- Tokens: `_tokens.scss`, `_colors.scss`, `_responsive.scss`, `_transitions.scss`

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

- `main` — versão estável
- `develop` — branch de trabalho ativa

## Checklist de Code Review

- [ ] Status é `int?` (não `string?`) em toda a cadeia
- [ ] `idUsuario` vem do JWT (não de query param)
- [ ] Props de entidade são `private set`
- [ ] Query params não contêm `undefined`
- [ ] Validators alinhados com regras das entidades
- [ ] Frontend envia status como number (1 ou 2)
- [ ] Repositorios frontend não enviam `idUsuario`
- [ ] Nenhum hex hardcoded fora do design system
- [ ] Mutação de categoria grava auditoria (`CategoriaHistorico`)
- [ ] Editar/excluir categoria valida dono/admin (`Erro.Permissao` → 403)
- [ ] DAS removido (não há mais auto-cálculo nem auto-geração)
- [ ] Leitura usa projeção (nomes display via `*Projecao`, não na entidade)
- [ ] Mutação re-busca projeção antes de mapear resposta
