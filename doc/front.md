# Frontend — Portal Financeiro

## Stack

- **Angular 22** — componentes **standalone** com **Signals**
- **Design system** próprio em `src/app/design-system/styles/` (tokens, mixins, variáveis)
- **Ícones**: Lucide Angular (`@lucide/angular`)
- **Componentes reutilizáveis** em `src/app/shared/components/`
- **Features** em `src/app/features/` (home, dashboard, receitas, despesas, lancamentos, contas, pessoas, clientes, parceiros, parcerias, contratos, categorias-receita, usuarios, login)

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
│   ├── contratos/       # + contrato-detalhe/
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
- **Busca no select**: `app-custom-select` já vem pesquisável por padrão (`searchable`, default `true`) + `searchPlaceholder="..."` e `searchThreshold` (default `5`); o campo de busca aparece só acima do limite e filtra sem diferenciar acentos nem maiúsculas/minúsculas. Desligue com `[searchable]="false"` nas listas fixas se precisar
- **Modal de lançamento (edição)**: botão **Salvar** sempre visível no rodapé de todos os passos (fluxo normal ou adicional, incluindo edição), porém desabilitado até todos os campos estarem preenchidos (`tudoConcluido`); Assim é possível finalizar antes do último passo (ex.: conta padrão já pré-selecionada); `Avançar` aparece só quando há passos à frente
- **Wizard do lançamento**: controle segmentado único e full-width no topo — uma faixa contínua com todos os passos em segmentos iguais (indicador numerado/check + rótulo na mesma linha); passo ativo como cartão branco com anel primário, concluído com check em `--color-success`, futuros apagados e separados por divisores sutis; mobile (≤767px) mostra só o indicador; conteúdo com altura natural (sem espaço vago); Conta e Recorrência ficam no mesmo cartão `.section--account`, sem linha divisória; largura via input `width` do `ModalComponent` (`640px` só no lançamento; demais modais seguem `580px`)
- **Cadastro rápido no wizard**: botões `+ Nova/Novo` ao lado do label criam categoria/subcategoria, categoria de serviço ou cliente sem sair do fluxo — o item criado é pré-selecionado e nada do digitado se perde (pais atualizam suas listas via `categoriaCriada`/`servicoCriado`/`clienteCriado`)
- **Categoria/subcategoria no wizard**: passo 0 lista categorias pai em `categoriasOptions` e, havendo filhas da selecionada, o campo Subcategoria usa `subcategoriasOptions` gravando em `idSubcategoria` via `selecionarSubcategoria` (parcerias só aparecem no passo de vínculo)
- **Descrição padrão no wizard**: no fluxo adicional de receita, ao selecionar o cliente (ou criar via `+`), a descrição é preenchida com o nome do cliente quando vazia; texto já digitado nunca é sobrescrito
 - **Contrato ou Parceria no wizard**: passo 1 do fluxo adicional com selects opcionais lado a lado (receita: Contrato + Parceria; despesa: só Parceria), mutuamente exclusivos via `onVinculoChange` (escolher um limpa o outro; backend impõe `RECEITA_VINCULO_DUPLO`); passo concluído sempre (vínculo opcional)
 - **Privacidade de valores**: `PrivacidadeService` (signal global + `localStorage portal-financeiro.privacidade`) + botão `app-privacidade-toggle` (olho) ao lado do título das telas com valores (dashboard, receitas, despesas, contratos, parcerias e detalhes); pipe impuro `valorMascarado` compõe `currencyBRL` e exibe `••••••` quando oculto; no dashboard, options dos gráficos viram `computed` (tooltips desligados, eixos com `••••••`, rótulos do canvas e sparklines zerados); inputs de edição nunca mascaram
  - **Contrato ou Parceria no wizard**: passo 1 do fluxo adicional com selects opcionais lado a lado (só receita tem Contrato; despesa mantém só Parceria); o select lista apenas registros ativos; edição/cópia segue fluxo de origem (tem `idCliente`/`servicos`/`idParceria`/`idContrato` → contrato)
- **Mover subcategoria**: só reestrutura (vale para novos lançamentos); lançamentos existentes mantêm o pai da época — o confirm avisa; ao abrir clone/edição com sub movida, o modal reconcilia para o pai atual com aviso (`reconciliarPaisComEstruturaAtual`, vale para categoria e blocos de serviço)
  - **Seletor de fluxo**: quando `fluxo-adicional-receita`/`fluxo-adicional-despesa` está habilitado, o botão **Nova receita/despesa** abre `FluxoSelectorModal` com dois quadrados — `Fluxo receita` / `Fluxo receita (contrato)` (e análogo para despesa) com ícone `file` × `briefcase-business`; a escolha define se o wizard roda em 3 ou 6 passos
 - **Permissões em tempo real**: ao salvar permissões do próprio usuário logado, `AuthService.atualizarPermissoes` atualiza o `signal` sem exigir relogin
  - **Grid de receitas**: coluna `Conta` substituída por `Contrato/Parceria` (`Contrato` com vínculo de contrato, `Sim` com vínculo de parceria, `—` sem); coluna `Categoria` exibe a categoria de serviço (`Servico → Sub`, separadas por vírgula; sem serviços, mostra a categoria de receita); totais com `Parceria` (soma das receitas vinculadas) e `Receita líquida` (recebido menos repasse ao parceiro: Σ valor × % por receita recebida vinculada) quando o fluxo adicional está ligado
  - **Grid de despesas**: ações com Pagar/Estornar (igual à receita: pendente mostra Pagar, paga mostra Estornar) + Copiar/Editar/Excluir; filtro de status com `Pagas`; totais com `Parceria` (soma das despesas vinculadas) quando o fluxo adicional está ligado; colunas Valor e Data com ordenação clicável (asc/desc)
- **Inputs de texto**: classe `input` do design system
- Forms: `ControlValueAccessor` para componentes reutilizáveis (CustomSelect)
- **Páginas parametrizadas**: `PessoaListagemComponent` (`tipo` Cliente/Parceiro) e `LancamentoListagemComponent` (`tipo` receita/despesa) — não duplicar páginas de listagem
- **SQL de lançamentos (backend)**: `LancamentoSql` é a base parametrizada; `ReceitaSql`/`DespesaSql` são wrappers finos
- SCSS com mixins do design system (`_page-layout.scss`, `_data-table.scss`, `_forms.scss`, `_responsive.scss`) — proibido copiar/colar estilos entre features
- **Listas**: scroll interno no grid via mixin `table-card-scroll` (`_data-table.scss`) — cabeçalho fixo (`thead` sticky) + paginação/rodapé fixos, rolando só as linhas (`max-height: 60vh`); no mobile mantém o scroll da página
- Nunca enviar `undefined` como query param — usar spread condicional
- Status enviado como `number` (1 ou 2), nunca string
- Repositórios NÃO enviam `idUsuario` nos params (vem do JWT)

## Design System

- Nunca hardcodar hex fora do design system
- Breakpoint mobile: 767px
- Transições com `will-change` para GPU acceleration
- Tokens: `_tokens.scss`, `_colors.scss`, `_responsive.scss`, `_transitions.scss`
- Scroll fino: mixin `scroll-thin` (`_scrollbar.scss`) — aplicar em toda área com rolagem interna (listas, legends, dropdowns)
- Tooltips de ajuda: atributo `data-tip` (CSS global em `styles.scss`, tooltip escuro; funciona com hover e foco de teclado)

## Telas / rotas

| Rota | Feature | Descrição |
|------|---------|-----------|
| `/login` | login | Autenticação |
| `/dashboard` | dashboard | Cabeçalho com subtítulo + toggle Mensal/Anual + navegação de período + filtro Todas as contas (vale p/ mensal e anual). Resumo mensal: 4 KPIs (Receitas, Despesas, Lucro líquido, Fluxo de caixa) + card único com colapso conjunto: fechado mostra "Receitas x Despesas \| Outros indicadores"; aberto mostra dois cards internos com cabeçalho próprio (gráfico com filtro de série Receitas/Ambos/Despesas + seta de recolher; 6 indicadores em tiles compactos estilo StatusInvest com tooltip); gráfico alterna por nº de contas (1 = evolução mensal; 2+ = por conta do mês) com barras finas e espaçadas (`categoryPercentage`/`barPercentage`); previsão incorporada do mês corrente em diante (realizado + regras vigentes descontando o materializado); na ordem: KPIs, gráfico + indicadores, Distribuição por categoria e subcategoria (toggle Receitas/Despesas no cabeçalho da seção, visível só com ela aberta, com donuts de Categorias e Subcategorias lado a lado), Contas bancárias (lucro líquido e % do total). Ver glossário em [Indicadores do dashboard mensal](#indicadores-do-dashboard-mensal). Visão anual: 4 KPIs na mesma fileira do mensal (Receitas, Despesas e Saldo com variação vs ano anterior e sparkline dos 12 meses; Média pró-rata usa a mesma estrutura, sem spark), gráfico de 12 meses em largura total com barras de Receitas e Despesas mais linhas de Saldo e Saldo acumulado no mesmo eixo, Distribuição por categoria e subcategoria com donuts lado a lado e toggle Receitas/Despesas acima de Por conta, Por conta com o mesmo layout mensal (avatar do banco, lucro líquido, % do total e linha Total). |
| `/receitas` | receitas | Lançamentos de receita (avulsas e recorrentes) |
| `/despesas` | despesas | Lançamentos de despesa |
| `/contas` | contas | Contas bancárias |
| `/categorias` | categorias-receita | Categorias compartilhadas (com subcategorias) |
| `/clientes` | clientes | Cadastro de clientes (tipo Cliente) |
| `/parceiros` | parceiros | Cadastro de parceiros (tipo Parceiro) |
| `/parcerias` | parcerias | Cadastro de parcerias (nome + parceiro + cliente + valor + % do parceiro) com visão de falta receber/pagar; total pago no mês com navegação de período; status Ativo/Encerado via toggle na linha + filtro de situação; encerrar exige faltas zeradas |
| `/parcerias/:id` | parceria-detalhe | Detalhe da parceria: resumo (partes, recebido, pago, faltas) + entradas (receitas) + saídas (despesas) |
| `/contratos` | contratos | Cadastro de contratos (nome + cliente + valor, sem parceiro) com falta receber; status Ativo/Encerado via toggle na linha + filtro de situação; encerrar exige falta receber zerada |
| `/contratos/:id` | contrato-detalhe | Detalhe do contrato: cliente + recebido/falta receber + entradas (receitas) |
| `/usuarios` | usuarios | Usuários e permissões (admin). Admin possui acesso total (bypass) a parcerias, contratos e demais telas; todos os botões e toggles são editáveis e os níveis ficam registrados, valendo caso o perfil seja alterado. Banner "Acesso total" explica a regra |
| `/testes` | testes | QA técnico (admin, fora do menu, só URL direta): semáforo develop → main, regras R1–R6 com cenários, saúde do banco, débitos e botão Atualizar. Consome `GET /api/diagnostico` |

### Indicadores do dashboard mensal

KPIs (fileira principal, com variação % vs mês anterior e sparkline):

| KPI | O que responde | Cálculo |
|-----|----------------|---------|
| Receitas | Quanto faturou/recebeu | Realizado + previsto das regras vigentes no mês |
| Despesas | Quanto consumiu | Realizado + previsto das regras vigentes no mês |
| Lucro líquido | Resultado econômico (quanto sobrou) | Receitas − Despesas |
| Fluxo de caixa | Movimentação efetiva de dinheiro (como terminou o caixa) | Recebido − Pago (só realizado, sem previsão) |

Outros indicadores (painel fixo ao lado do gráfico mensal, 6 tiles compactos com tooltip explicativo via `data-tip` — mesmo padrão dos 4 KPIs):

| Indicador | Cálculo |
|-----------|---------|
| Contratos ativos | Quantidade de contratos ativos + soma do falta receber |
| Receita recorrente | Receitas do mês geradas por regras de repetição (R$ + % da receita) |
| Despesas recorrentes | Despesas do mês geradas por regras de repetição, o custo fixo (R$ + % das despesas) |
| Parcerias | Total pago a parcerias no mês (nota: valor já incluso na despesa) + variação %, contagem |
| Margem líquida | Lucro líquido ÷ Receita (%) |
| Ponto de equilíbrio | Despesas do mês (= receita necessária para cobrir os gastos); distância % = (Receitas − Despesas) ÷ Despesas |

Regras gerais: do mês corrente em diante os valores incorporam previsão (regras vigentes descontando o já materializado por `IdRegra`); meses passados mostram só o realizado. Variação % ancora no mês anterior (`—` sem base).

### Dashboard anual

- Os quatro KPIs usam a mesma fileira do mensal: Receitas, Despesas, Saldo do ano e Média mensal (saldo).
- Receitas, Despesas e Saldo mostram variação % contra o ano anterior e sparkline dos 12 meses.
- Média mensal (saldo) usa o valor pró-rata e a quantidade de meses considerados.
- O gráfico anual combina barras de Receitas e Despesas com linhas de Saldo e Saldo acumulado no mesmo eixo; a tabela redundante Resumo mês a mês foi removida.
- O card Distribuição por categoria e subcategoria é o mesmo layout mensal, fica acima de Por conta e mantém o toggle Receitas/Despesas.
- Por conta usa a tabela mensal, com avatar do banco, lucro líquido, % do total de receitas e linha Total.

### Menu lateral

- **Dashboard**, **Receitas**, **Despesas**, **Parcerias** e **Contratos** ficam no nível principal. **Parcerias** e **Contratos** usam permissão regular (Leitura/Escrita) como Clientes e Parceiros, com bypass explícito para admin (`temPermissao(...) || isAdmin()` na sidebar e `temPermissao` com `isAdmin => true` no guard). Parcerias são vinculadas em Receitas/Despesas via `IdParceria` e exibem saldo (falta receber/pagar); contratos são vinculados em Receitas via `IdContrato` (no máximo um vínculo por receita) e exibem falta receber.
- **Configurações** é um grupo colapsável que reúne, nesta ordem: **Contas**, **Categorias**, **Cliente**, **Parceiro** e **Usuários** (admin).
- O ícone `user-key` fica reservado para quando o item **Permissões** voltar.
