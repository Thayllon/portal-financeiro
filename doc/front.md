# Frontend — Portal Financeiro

## Stack

- **Angular 22** — componentes **standalone** com **Signals**
- **Design system** próprio em `src/app/design-system/styles/` (tokens, mixins, variáveis)
- **Ícones**: Lucide Angular (`@lucide/angular`)
- **Componentes reutilizáveis** em `src/app/shared/components/`
- **Features** em `src/app/features/` (home, dashboard, receitas, despesas, lancamentos, contas, pessoas, clientes, parceiros, parcerias, contratos, processos, categorias-receita, usuarios, login, testes)

## Como rodar / buildar / testar

```bash
# Instalar dependências (primeira vez)
npm install

# Rodar (http://localhost:4200)
npm start        # = ng serve

# Build de produção (Vercel/Render — `environment.prod.ts` com a URL da API)
npm run build

# Build para containers Docker (compose local / Oracle — `environment.docker.ts`, `apiUrl: '/api'`)
npm run build:docker

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
│   ├── processos/       # + processo-detalhe/
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
- **Leitura = somente ver**: usuário com nível **Leitura** num módulo acessa a tela em modo leitura — botões de criação/edição/exclusão (e ações como receber/pagar, estornar, encerrar/reativar, definir padrão, mover subcategoria) ficam ocultos, inclusive a coluna "Ações"; o estado vazio mostra aviso "Você tem acesso somente a leitura nesta tela". O nível **Escrita** é exigido para exibir essas ações (via `auth.temPermissao(modulo, NivelPermissao.Escrita)`), alinhado ao `[RequerPermissaoEscrita]` do backend.
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
| `/dashboard` | dashboard | Cabeçalho com subtítulo + toggle Mensal/Anual + navegação de período + filtro Todas as contas (vale p/ mensal e anual). Conceito principal **caixa (realizado)** contado pela `DataRealizacao`. Resumo mensal: 4 KPIs (Recebido, Pago, Saldo do mês, Acumulado no ano) + card único com colapso conjunto: fechado mostra "Recebido x Pago \| Outros indicadores"; aberto mostra dois cards internos com cabeçalho próprio (gráfico com filtro de série Recebido/Ambos/Pago + seta de recolher; 6 indicadores em tiles compactos estilo StatusInvest com tooltip); gráfico alterna por nº de contas (1 = evolução mensal; 2+ = por conta do mês) com barras finas e espaçadas (`categoryPercentage`/`barPercentage`); na ordem: KPIs, gráfico + indicadores, Distribuição por categoria e subcategoria (toggle Receitas/Despesas no cabeçalho da seção, visível só com ela aberta, com donuts de Categorias e Subcategorias lado a lado), Contas bancárias (Recebido/Pago/Saldo de caixa e % do total de recebido). Ver glossário em [Indicadores do dashboard mensal](#indicadores-do-dashboard-mensal). Visão anual: 4 KPIs na mesma fileira do mensal (Recebido, Pago e Saldo do ano em caixa com variação vs ano anterior e sparkline dos 12 meses; Média mensal de caixa usa a mesma estrutura, sem spark), gráfico de 12 meses em largura total com barras de Recebido e Pago mais linha de Saldo acumulado no mesmo eixo, Distribuição por categoria e subcategoria com donuts lado a lado e toggle Receitas/Despesas acima de Por conta, Por conta com o mesmo layout mensal (avatar do banco, Recebido/Pago/Saldo de caixa, % do total e linha Total). |
| `/receitas` | receitas | Lançamentos de receita (avulsas e recorrentes) |
| `/despesas` | despesas | Lançamentos de despesa |
| `/contas` | contas | Contas bancárias |
| `/categorias` | categorias-receita | Categorias compartilhadas (com subcategorias) |
| `/clientes` | clientes | Cadastro de clientes (tipo Cliente) |
| `/parceiros` | parceiros | Cadastro de parceiros (tipo Parceiro) |
| `/parcerias` | parcerias | Cadastro de parcerias (nome + parceiro + cliente + valor + % do parceiro) com visão de falta receber/pagar; status Ativo/Encerado via toggle na linha + filtro de situação; encerrar exige faltas zeradas |
| `/parcerias/:id` | parceria-detalhe | Detalhe da parceria: resumo (partes, recebido, pago, faltas) + entradas (receitas) + saídas (despesas) |
| `/contratos` | contratos | Cadastro de contratos (nome + cliente + valor, sem parceiro) com falta receber; status Ativo/Encerado via toggle na linha + filtro de situação; encerrar exige falta receber zerada |
| `/contratos/:id` | contrato-detalhe | Detalhe do contrato: cliente + recebido/falta receber + entradas (receitas) |
| `/processos` | processos | Processos com etapas do mundo real (nome + descrição, progresso x/y • %; sem vínculo obrigatório) |
| `/processos/:id` | processo-detalhe | Detalhe do processo: progresso + etapas (concluir/estornar, reordenar) |
| `/usuarios` | usuarios | Usuários e permissões (admin). Admin possui acesso total (bypass) a parcerias, contratos e demais telas; todos os botões e toggles são editáveis e os níveis ficam registrados, valendo caso o perfil seja alterado. Banner "Acesso total" explica a regra. Excluir usuário com dados abre `ConfirmDialog` em modo texto: mostra a mensagem de erro da API + o total de registros vinculados (via `GET /usuarios/{id}/vinculos`) e só confirma se o usuário digitar `SIM` (qualquer caixa); envia `DELETE ?cascata=true&confirmacao=sim` |
| `/testes` | testes | QA técnico (admin, fora do menu, só URL direta): semáforo develop → main, regras R1–R6 com cenários, saúde do banco, débitos e botão Atualizar. Consome `GET /api/diagnostico` |

### Regressão de permissões (perfil restrito)
Script: `scripts/test-perfil-restrito.ps1` (exige a API no ar; não cria dados; restaura os níveis ao final). Cobre `receitas`, `despesas`, `parcerias` e `contratos` nos níveis 0/1/2: reflete no login, leitura de lista liberada (só exige auth) e escrita bloqueada com 403 nos níveis 0/1.

Checklist manual (sair/entrar do usuário teste a cada troca — permissões congelam no login, `auth.service.ts`):
| Nível | Menu | URL direta | Botão incluir |
|-------|------|------------|---------------|
| 0 – não pode ver | oculto | redireciona para `/` | oculto |
| 1 – leitura | visível, lista carrega | abre | oculto |
| 2 – escrita | visível | abre | visível |

Nomes de módulo são tipados (`ModuloPermissao` em `core/models/permissao.model.ts`, sempre no plural: `receitas`, `despesas`, ...). Nunca usar o `tipo` singular de listagem (`receita`/`despesa`) como módulo — o compilador barra (TS2345).

### Indicadores do dashboard mensal

KPIs (fileira principal, com variação % vs mês anterior e sparkline — exceto Acumulado no ano, sem pill). Conceito principal é **caixa (realizado)**, contado pela `DataRealizacao`:

| KPI | O que responde | Cálculo |
|-----|----------------|---------|
| Recebido | Quanto entrou de fato no mês | `totalRecebido` do `/dashboard` (realizado por `DataRealizacao`, sem previsão) |
| Pago | Quanto saiu de fato no mês | `totalPago` (realizado por `DataRealizacao`, sem previsão) |
| Saldo do mês | Resultado de caixa do mês | Recebido − Pago (`saldoRealizado`) |
| Acumulado no ano | Como está o caixa de janeiro até o mês selecionado | Recebido − Pago acumulado Jan→mês (via `GET /dashboard/anual`, `resumoPorMes[].mes <= mes`); verde ≥ 0, vermelho < 0; subtítulo `Jan–Mmm · recebido · pago` |

Outros indicadores (painel fixo ao lado do gráfico mensal, 6 tiles compactos com tooltip explicativo via `data-tip` — mesmo padrão dos 4 KPIs; os tiles **Contratos ativos** e **Parcerias** aparecem somente com a permissão especial **Outros indicadores** + leitura no módulo (`contratos`/`parcerias`) — sem ela, os slots mostram **Recebido** e **Pago** (realizado puro, sem previsão), mantendo 6 tiles; contagem fixa):

| Indicador | Cálculo |
|-----------|---------|
| Contratos ativos | Quantidade de contratos ativos + soma do falta receber |
| Receita recorrente | Receitas do mês geradas por regras de repetição (R$ + % da receita) |
| Despesas recorrentes | Despesas do mês geradas por regras de repetição, o custo fixo (R$ + % das despesas) |
| Parcerias | Total pago a parcerias no mês (nota: valor já incluso na despesa) + variação %, contagem |
| Margem líquida | Saldo de caixa do mês ÷ Recebido (%) |
| Previsão do mês | Regras vigentes ainda não lançadas no mês: receitas previstas, despesas previstas e saldo previsto (`totalReceitasPrevisto` − `totalDespesasPrevisto`); verde/vermelho pelo saldo |

Regras gerais: os KPIs/cards principais são sempre **caixa** (realizado pela `DataRealizacao`); a competência (por `Data`) segue apenas na distribuição por categoria, nos recorrentes e no total (denominador da taxa de realização). Variação % ancora no mês anterior (`—` sem base).

### Dashboard anual

- Os quatro KPIs usam a mesma fileira do mensal, em **caixa**: Recebido, Pago, Saldo do ano (caixa) e Média mensal (saldo de caixa).
- Recebido, Pago e Saldo mostram variação % contra o ano anterior (também em caixa) e sparkline dos 12 meses (recebido/pago/saldo realizado).
- Média mensal (saldo) = saldo de caixa do ano ÷ **meses com movimento** (recebido+pago > 0); subtítulo "Baseado em X meses".
- O gráfico anual combina barras de **Recebido** e **Pago** com a linha de **Saldo acumulado** (`saldoRealizadoAcumulado`) no mesmo eixo.
- A seção Outros indicadores (entre o gráfico e a Distribuição, mesmo padrão de tiles do mensal) tem 10 grupos: Fluxo de caixa anual (`saldoRealizado`), Receita/Despesa recorrente (`totalReceitasRecorrentes/totalDespesasRecorrentes` do `/dashboard/anual` + % do total), Contratos ativos (reuse do mensal), Parcerias no ano (`resumoParcerias.totalPago` + qtd), Margem líquida (`saldoRealizado ÷ totalRecebido`), Ponto de equilíbrio (total pago + distância % `saldoRealizado ÷ totalPago`), Melhor/pior mês (max/min de `resumoPorMes[].saldoRealizado`), Taxa de realização (`totalRecebido÷totalReceitas`, `totalPago÷totalDespesas`), Médias mensais (recebido e pago ÷ `mesesConsiderados`), Top categoria do ano (maior de `distribuicaoReceitas/Despesas`) e Previsão restante (`previsaoRestanteAno` somado).
- O card Distribuição por categoria e subcategoria é o mesmo layout mensal, fica abaixo de Outros indicadores e acima de Por conta e mantém o toggle Receitas/Despesas.
- Na visão anual, os tiles **Contratos ativos** e **Parcerias no ano** seguem a mesma regra da flag **Outros indicadores** (sem ela, viram **Recebido no ano** e **Pago no ano**), mantendo 10 tiles.
- Por conta usa a tabela mensal, com avatar do banco, **Recebido/Pago/Saldo de caixa**, % do total de recebido e linha Total.

### Menu lateral

- **Dashboard**, **Receitas**, **Despesas**, **Parcerias**, **Contratos** e **Processos** ficam no nível principal. **Parcerias**, **Contratos** e **Processos** usam permissão regular (Leitura/Escrita) como Clientes e Parceiros, com bypass explícito para admin (`temPermissao(...) || isAdmin()` na sidebar e `temPermissao` com `isAdmin => true` no guard). Parcerias são vinculadas em Receitas/Despesas via `IdParceria` e exibem saldo (falta receber/pagar); contratos são vinculados em Receitas via `IdContrato` (no máximo um vínculo por receita) e exibem falta receber; processos são cadastrados com nome + descrição (vínculo com parceria/contrato é opcional e não aparece na tela) e exibem progresso das etapas.
- **Configurações** é um grupo colapsável que reúne, nesta ordem: **Contas**, **Categorias**, **Cliente**, **Parceiro** e **Usuários** (admin).
- O ícone `user-key` fica reservado para quando o item **Permissões** voltar.
