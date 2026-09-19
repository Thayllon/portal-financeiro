# Frontend — Portal Financeiro

## Stack

- **Angular 22** — componentes **standalone** com **Signals**
- **Design system** próprio em `src/app/design-system/styles/` (tokens, mixins, variáveis)
- **Ícones**: Lucide Angular (`@lucide/angular`)
- **Componentes reutilizáveis** em `src/app/shared/components/`
- **Features** em `src/app/features/` (home, dashboard, receitas, despesas, lancamentos, contas, pessoas, clientes, parceiros, parcerias, categorias-receita, usuarios, login)

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

# Lint (ESLint, 0 erros; warnings = débito documentado)
npm run lint
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
│   ├── guards/          # authGuard, adminGuard, permissionGuard
│   └── interceptors/    # authInterceptor, errorInterceptor
├── design-system/
│   └── styles/          # Tokens, mixins, variáveis CSS
├── features/
│   ├── home/
│   ├── dashboard/
│   ├── receitas/        # wrapper fino → LancamentoListagemComponent
│   ├── despesas/        # wrapper fino → LancamentoListagemComponent
│   ├── lancamentos/     # LancamentoListagemComponent (tipo receita/despesa)
│   ├── contas/
│   ├── pessoas/         # PessoaListagemComponent (tipo Cliente/Parceiro)
│   ├── clientes/        # wrapper fino → PessoaListagemComponent
│   ├── parceiros/       # wrapper fino → PessoaListagemComponent
│   ├── parcerias/       # + parceria-detalhe/
│   ├── categorias-receita/
│   ├── usuarios/
│   └── login/
└── shared/
    ├── components/      # Componentes reutilizáveis (LancamentoModal, CustomSelect...)
    ├── composables/     # useListPagination
    ├── constants/       # PAGE_SIZE_OPTIONS
    ├── directives/      # currency-input
    ├── pipes/           # CurrencyBRLPipe
    ├── services/        # ConfirmService
    └── utils/           # api-error
```

## Padrões de código

Regras completas no [AGENTS.md](../AGENTS.md). Resumo:

- **Signals**: `signal()` para estado, `computed()` para derivados
- Componentes standalone com `imports` explícitos
- Ícones Lucide: `<svg lucideIcon="nome" [size]="16" />`
- **Selects/Dropdowns**: SEMPRE `app-custom-select` (padrão reutilizável) — proibido `<select>` nativo
- **Busca no select**: `app-custom-select` aceita `[searchable]="true"` + `searchPlaceholder="..."`; o campo de busca aparece só quando há mais de 5 opções e filtra sem diferenciar acentos nem maiúsculas/minúsculas (ex.: Cliente na Nova Receita)
- **Modal de lançamento (edição)**: com item em edição, o botão **Salvar** aparece em todos os passos (cópia sem id mantém o wizard)
- **Cadastro rápido no wizard**: botões `+ Nova/Novo` ao lado do label criam categoria/subcategoria, categoria de serviço ou cliente sem sair do fluxo — o item criado é pré-selecionado e nada do digitado se perde (pais atualizam suas listas via `categoriaCriada`/`servicoCriado`/`clienteCriado`)
 - **Parceria no wizard**: passo dedicado só no fluxo adicional; em Dados Gerais o campo aparece só no fluxo simples (despesas)
 - **Seletor de fluxo**: quando `fluxo-adicional-receita`/`fluxo-adicional-despesa` está habilitado, o botão **Nova receita/despesa** abre `FluxoSelectorModal` com dois quadrados — `Fluxo receita` / `Fluxo receita (contrato)` (e análogo para despesa) com ícone `file` × `briefcase-business`; a escolha define se o wizard roda em 3 ou 6 passos; edição/cópia segue fluxo de origem (tem `idCliente`/`servicos`/`idParceria` → contrato)
 - **Permissões em tempo real**: ao salvar permissões do próprio usuário logado, `AuthService.atualizarPermissoes` atualiza o `signal` sem exigir relogin
 - **Grid de receitas**: coluna `Conta` substituída por `Parceria` (`Sim` com vínculo, `—` sem); totais com `Parceria` (soma das receitas vinculadas) quando o fluxo adicional está ligado
- **Inputs de texto**: classe `input` do design system
- Forms: `ControlValueAccessor` para componentes reutilizáveis (CustomSelect)
- **Páginas parametrizadas**: `PessoaListagemComponent` (`tipo` Cliente/Parceiro) e `LancamentoListagemComponent` (`tipo` receita/despesa) — não duplicar páginas de listagem
- **SQL de lançamentos (backend)**: `LancamentoSql` é a base parametrizada; `ReceitaSql`/`DespesaSql` são wrappers finos
- SCSS com mixins do design system (`_page-layout.scss`, `_data-table.scss`, `_forms.scss`, `_responsive.scss`) — proibido copiar/colar estilos entre features
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
| `/dashboard` | dashboard | Resumo mensal (por conta/categoria + previsão 3 meses) e visão anual (5 KPIs com variação vs ano anterior + média pró-rata, bar 12 meses, donut receitas/despesas por categoria/subcategoria, resumo mês a mês, previsão restante do ano via regras, por conta e card parcerias com link) |
| `/receitas` | receitas | Lançamentos de receita (avulsas e recorrentes) |
| `/despesas` | despesas | Lançamentos de despesa |
| `/contas` | contas | Contas bancárias |
| `/categorias` | categorias-receita | Categorias compartilhadas (com subcategorias) |
| `/clientes` | clientes | Cadastro de clientes (tipo Cliente) |
| `/parceiros` | parceiros | Cadastro de parceiros (tipo Parceiro) |
| `/parcerias` | parcerias | Cadastro de parcerias (nome + parceiro + cliente + valor + % do parceiro) com visão de falta receber/pagar |
| `/parcerias/:id` | parceria-detalhe | Detalhe da parceria: resumo (partes, recebido, pago, faltas) + entradas (receitas) + saídas (despesas) |
| `/usuarios` | usuarios | Usuários e permissões (admin) |

### Menu lateral

- **Dashboard**, **Receitas**, **Despesas** e **Parcerias** ficam no nível principal. **Parcerias** usa permissão regular (Leitura/Escrita) como Clientes e Parceiros. Parcerias são vinculadas em Receitas/Despesas via `IdParceria` e exibem saldo (falta receber/pagar).
- **Configurações** é um grupo colapsável que reúne, nesta ordem: **Contas**, **Categorias**, **Cliente**, **Parceiro** e **Usuários** (admin).
- O ícone `user-key` fica reservado para quando o item **Permissões** voltar.
