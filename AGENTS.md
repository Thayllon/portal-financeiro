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

# Testes backend (xUnit + FluentAssertions)
dotnet test PortalFinanceiro.API.slnx

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
- **Documentação**: em `doc/` — `README.md` é o índice; arquivos por área (`back.md`, `front.md`, `banco.md`, `infra.md`, `primeiros-passos.md`). **Manter sempre atualizada** — toda mudança que altere API, schema, telas ou conceitos deve atualizar a doc correspondente **na mesma entrega**. Doc desatualizada = feature incompleta.

## Padrões de Código

> **Antes de criar qualquer componente, utilitário, estilo, serviço ou primitivo**: procure implementações equivalentes em `design-system/`, `shared/components/`, `shared/services/`, `core/` e nas páginas existentes. Reutilizar ou estender o que já existe **não requer confirmação**. **Duplicar código ou refatorar código existente sempre requer o aval do usuário** — sinalize a situação e deixe a decisão com ele.

### Duplicação e conformidade de padrão (detectar e sinalizar)

- **Papel do agente = detector/sinalizador, não refatorador.** Antes de implementar, verifique se a solução planejada vai **duplicar código existente** ou **divergir de padrões já decididos** neste arquivo.
- Heurísticas de detecção: comparar com "irmãos" na mesma pasta (`*AppService.cs`, `*Repository.cs`, `features/*`); procurar shapes copiados (`page`/`table-card`/`field`, CRUD de `AppService`, `Repository` Dapper, dropdowns); rodar `Get-ChildItem` ou `grep` para mapear padrões repetidos.
- Ao detectar duplicação/divergência, **sinalize ao usuário** com: (1) o que será duplicado/desviado, (2) o equivalente existente que poderia ser reutilizado/estendido, (3) a opção de refatorar.
- **Nunca execute refactor** (extrair base genérica, consolidar, renomear) por iniciativa própria — a decisão de refatorar ou duplicar é sempre do usuário.

### Comentários

- **Não adicione comentários ao código.** Prefira código autoexplicativo e bons nomes.
- Exceção: comentário **extremamente útil** — regra de negócio/edge case não óbvio — curto, explicando **o porquê** (nunca o quê).
- Proibido: TODOs/FIXMEs, blocos de autoria/data, comentários que repetem o código.

### Boas Práticas (regras curtas)

- **Nomes e código autoexplicativo**: nomes claros e consistentes em pt-BR; métodos pequenos; classe com uma responsabilidade.
- **Async correto**: `async/await` em toda a cadeia; proibido `.Result`/`.Wait()` e `async void`.
- **Sem código morto**: sem imports/`using` não usados, sem propriedades/métodos sem uso.
- **Segredos e segurança**: nunca commitar senhas, tokens ou connection strings (usar `.env`/variáveis); não logar dados sensíveis.

### Linguagem e mensagens

- Mensagens ao usuário (erros `Erro.*`, validações, notificações, placeholders, alertas) sempre em **português, com acentuação correta** (UTF-8).
- Nunca remover acentos ("nao", "voce") nem usar caracteres especiais desnecessários (emojis, `\n` no texto exibido, entidades HTML).
- Ex.: "Não é possível excluir…" (padrão já usado no projeto).

### Arquivos

- Não deixar arquivos "lixo": ao final da feature, remover classes/serviços/config/repos **não referenciados**.
- Não commitar temporários (`.log`, `.tmp`, `.bak`, duplicados) nem arquivos criados "por precaução".
- Arquivo sem uso = delete.

### Migrações (scripts SQL)

- Não **empilhar** scripts incrementalmente sem revisar os anteriores.
- Ao criar migração nova, avaliar se scripts antigos ficaram obsoletos ou já foram absorvidos pelo `001_CriarTabelas` (from-scratch) — se sim, **propor** (caso a caso, com aval do usuário) remoção/extração/nova numeração, mantendo DbUp/journal coerente.
- `001`/`099` refletem o schema final; incrementais só migram banco já criado.

### Mapeamento de Erro → HTTP

Services retornam `Result<T>` / `Erro.*` (`ETipoErro`). O mapeamento para HTTP é centralizado em `ErroHttp`:

| Tipo           | HTTP |
|----------------|------|
| Validacao      | 400  |
| Negocio        | 422  |
| NaoEncontrado  | 404  |
| Conflito       | 409  |
| Permissao      | 403  |
| Timeout        | 504  |
| Externo        | 502  |
| Infraestrutura| 500  |

### Contrato de erro (proibido 400 genérico)

Toda resposta de erro da API deve seguir o contrato único tipado `{ codigo, mensagem, tipo }` (objeto `Erro`). O frontend espera esse shape (consumido via `ApiResponse`/interceptor).

- **Nunca** retorne `BadRequest(new { mensagem = "..." })` nem qualquer objeto anônimo/genérico. Isso produz um "400 genérico" sem `codigo`/`tipo` e quebra o contrato de erro consumido pelo frontend.
- Em controllers, **sempre** devolva o `Result<T>` via `ApiResponse(result)` (método do `BaseController`), que mapeia `Erro.Tipo` → HTTP status (via `ErroHttp`) e serializa o `Erro` completo.
- Para guardas/cláusulas de entrada sem `Result`, retorne o `Erro` tipado: `return BadRequest(Erro.Validacao("CODIGO", "mensagem"))` (consistência com `Unauthorized(Erro.Permissao(...))`).
- Status corretos vêm do `ETipoErro` (`Validacao=400`, `Negocio=422`, `NaoEncontrado=404`, `Conflito=409`, `Permissao=403`, ...). Não invente status "na mão".
- FluentValidation e `InvalidModelStateResponseFactory` já devolvem `Erro.Validacao(...)` — reutilize, não contorne.

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
- **Lucide icons**: `<svg lucideIcon="nome" [size]="16" />` — ícones devem estar registrados em `provideLucideIcons()` em `app.config.ts`
- **Forms**: ControlValueAccessor para componentes reutilizáveis (CustomSelect)
- **SCSS**: Usar mixins do design system (`_page-layout.scss`, `_data-table.scss`, `_forms.scss`, `_responsive.scss`, `_transitions.scss`) — NÃO copiar/colar estilos inline entre features
- **Params**: NUNCA enviar `undefined` como query param — usar spread condicional
- **Status**: Enviar como `number` (1 ou 2), NÃO como string
- **Repositorios**: NÃO incluir `idUsuario` nos params (vem do JWT)
- **Categoria/Regra repositories**: usar classe base com parâmetro `rota` para evitar duplicação
- **LancamentoRepository**: usar `LancamentoFiltros` compartilhado (alias `ReceitaFiltros`/`DespesaFiltros`)
- **Listas**: Usar `useListPagination` + `ListPaginationComponent` em toda listagem de dados
- **Interceptor de erro**: `errorInterceptor` registrado em `app.config.ts` para tratar 401 → logout automático

### Design System

- Nunca hardcodar hex fora do design system
- Responsividade: breakpoint mobile em 767px
- Transições: usar `will-change` para GPU acceleration
- Tokens: `_tokens.scss`, `_colors.scss`, `_responsive.scss`, `_transitions.scss`
- **SCSS de features**: Usar mixins de `_page-layout.scss` (`page-wrapper`, `page-header`, `page-title`, `card`), `_data-table.scss` (`data-table`, `cell-name`, `cell-actions`, `action-btn`), `_forms.scss` — NÃO copiar/colar estilos inline entre features

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
│   ├── pessoas/
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

## Limpeza de Branch

- Depois que uma branch de trabalho for **mergeada na `develop`**, ela pode (e deve) ser **excluída**: `git branch -d <nome-da-branch>`.

## Checklist de Code Review

- [ ] Status é `int?` (não `string?`) em toda a cadeia
- [ ] `idUsuario` vem do JWT (não de query param)
- [ ] Props de entidade são `private set`
- [ ] Query params não contêm `undefined`
- [ ] Validators alinhados com regras das entidades
- [ ] Frontend envia status como number (1 ou 2)
- [ ] Repositorios frontend não enviam `idUsuario`
- [ ] Nenhum hex hardcoded fora do design system
- [ ] Nenhum `BadRequest(new { mensagem = ... })` ou objeto anônimo (400 genérico proibido); usar `ApiResponse(result)` ou `Erro.*` tipado
- [ ] Mutação de categoria grava auditoria (`CategoriaHistorico`)
- [ ] Editar/excluir categoria valida dono/admin (`Erro.Permissao` → 403)
- [ ] DAS removido (não há mais auto-cálculo nem auto-geração)
- [ ] Leitura usa projeção (nomes display via `*Projecao`, não na entidade)
- [ ] Mutação re-busca projeção antes de mapear resposta
- [ ] `dotnet test` passando (backend)
- [ ] Listas usam `useListPagination` + `ListPaginationComponent`
- [ ] SCSS usa mixins do design system (sem duplicação entre features)
- [ ] Ícones Lucide estão registrados em `provideLucideIcons()`
- [ ] `errorInterceptor` registrado em `app.config.ts`
- [ ] Duplicação/fuga de padrão verificada e **sinalizada** quando detectada (nenhuma cópia ou refactor sem confirmação)
- [ ] Sem comentários novos, salvo extremamente úteis (explicam o porquê)
- [ ] Sem segredos/conn strings commitados; sem dados sensíveis em log
- [ ] Mensagens em pt-BR, com acentuação, sem caracteres especiais indevidos
- [ ] Sem arquivos lixo/não referenciados (classes, config, temporários)
- [ ] Scripts SQL revisados (não apenas empilhados); sequência coerente
- [ ] Docs (`back`/`front`/`banco`/`README`) atualizadas na mesma entrega
