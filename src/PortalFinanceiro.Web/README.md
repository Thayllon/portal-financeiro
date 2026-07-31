# PortalFinanceiro Web

Frontend Angular 22 do Portal Financeiro.

## Desenvolvimento

```bash
# Instalar dependências
npm install

# Rodar em desenvolvimento
ng serve

# Build de produção
ng build

# Testes
ng test
```

## Design System

O projeto utiliza um design system centralizado em `src/app/design-system/styles/`:

| Arquivo | Função |
|---------|--------|
| `_tokens.scss` | Variáveis CSS: spacing, border-radius, shadows, breakpoints |
| `_colors.scss` | Cores do tema: primary, success, error, warning, info |
| `_theme.scss` | Combina tokens e cores |
| `_responsive.scss` | Mixins: `mobile`, `tablet-up`, `lg-up` |
| `_transitions.scss` | Animações: fade, slide, scale, shimmer |
| `_page-layout.scss` | Mixins de layout: page-wrapper, card, toolbar, totals |
| `_data-table.scss` | Mixins de tabela responsiva |
| `_form-fields.scss` | Mixins de formulário: field, input, select, checkbox |

## Componentes Compartilhados

| Componente | Caminho | Descrição |
|------------|---------|-----------|
| `CustomSelectComponent` | `shared/components/custom-select.component.ts` | Select customizado com ControlValueAccessor |
| `PageComponent` | `shared/components/page.component.ts` | Wrapper de página com header (ícone + título) |
| `SectionHeaderComponent` | `shared/components/section-header.component.ts` | Header com ícone Lucide + título + botão de ação |
| `ListPaginationComponent` | `shared/components/list-pagination.component.ts` | Paginação: "1-10 de 50 \| Anterior \| Próximo" |
| `ModalComponent` | `shared/components/modal.component.ts` | Modal animado com footer |
| `TabsComponent` | `shared/components/tabs.component.ts` | Abas com indicador ativo |
| `StatusBadgeComponent` | `shared/components/status-badge.component.ts` | Badge de status (pendente, realizado, ativo, inativo) |
| `MonthNavComponent` | `shared/components/month-nav.component.ts` | Navegação mês/ano |
| `SkeletonComponent` | `shared/components/skeleton.component.ts` | Loading skeleton com shimmer |
| `ToastComponent` | `shared/components/toast.component.ts` | Toast de notificação |
| `ConfirmDialogComponent` | `shared/components/confirm-dialog.component.ts` | Diálogo de confirmação |
| `EmptyStateComponent` | `shared/components/empty-state.component.ts` | Estado vazio |

## Composables

| Composable | Caminho | Descrição |
|------------|---------|-----------|
| `useListPagination` | `shared/composables/use-list-pagination.composable.ts` | Paginação client-side com signals |

## Padrões

- **Ícones**: Lucide Angular (`@lucide/angular`) — usar `[lucideIcon]="'nome-do-icone'"`
- **Forms**: ControlValueAccessor para componentes reutilizáveis
- **Signals**: Todos os componentes usam Angular signals para estado
- **Responsividade**: Breakpoints em `_responsive.scss` — mobile (max-width: 767px), tablet (min-width: 768px)
- **Animações**: Transições suaves com `will-change` para performance GPU
