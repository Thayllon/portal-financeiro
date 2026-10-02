# Banco de dados — Portal Financeiro

## Provider

O aplicativo roda **somente em PostgreSQL**. O SQL Server local foi descontinuado
como fonte de dados: o banco `PortalFinanceiro` (LocalDB) permanece apenas como
**backup** e não é mais lido nem modificado pelo código.

| Banco | Conexão | Papel |
|-------|---------|-------|
| PostgreSQL local (dev) | `localhost:5432/portal_financeiro` | Banco real de desenvolvimento (réplica em `D:\projetos\postgres-replica`) |
| PostgreSQL Neon | `ep-*.neon.tech` | Produção (`neondb`) e réplica com dados (`neondb_bravo`) |

## Scripts

Os scripts vivem em `scripts/postgres/` e são aplicados **manualmente** (psql / SQL Editor).
Não há DbUp nem ferramenta de migração no repositório (removidos junto com a infra SQL Server).

| Script | Conteúdo |
|--------|----------|
| `001_CriarTabelas.sql` | Schema unificado completo (todas as tabelas, índices, FKs) — banco novo |
| `003_FluxoAdicionalDespesa.sql` | Incremental idempotente: `Despesa.IdCliente` + `DespesaServico` |
| `004_ContaPadrao.sql` | Incremental idempotente: `ContaBancaria.EhPadrao` |
| `005_Contratos.sql` | Incremental idempotente: `Contrato` + `Receita.IdContrato` + módulo `contratos` |
| `006_ContratoRecorrente.sql` | Incremental idempotente: `Contrato.EhRecorrente` + `Contrato.IdRegra` |
| `100_DDL_AtualizarEstrutura.sql` | DDL idempotente p/ Neon desatualizado (cria tabelas/colunas/índices/FKs faltantes) |
| `103_CheckEnums.sql` | `CHECK constraints` dos enums (tipo/nível/status) |
| `104_PermissaoCategoriasGranulares.sql` | Divide `categorias` em 3 módulos granulares preservando níveis |
| `105_AuditoriaAtor.sql` | `CriadoPor`/`AlteradoPor` nas tabelas de domínio |
| `106_MotorProcessos.sql` | Motor de processos: `ModeloProcesso`/`ModeloEtapa`/`ModeloItem`, `Processo`/`ProcessoEtapa`, `ProcessoEtapaItem` e `ProcessoAnexo`; colunas novas em `Processo`/`ProcessoEtapa`; módulo `processos` e seed do modelo "Regularização de imóvel". Idempotente |

**Banco novo — ordem completa:** `001` → `003` → `004` → `005` → `006` → `100` → `103` → `104` → `105` → `106`.

> A sincronização **local → Neon** é feita pelo `sincronizar-banco.ps1` (origem = PostgreSQL
> local; `neondb` recebe schema via `pg_dump --schema-only`, `neondb_bravo` é recriado com
> schema + dados). O script opera somente em PostgreSQL e **rejeita qualquer host que pareça
> SQL Server**; o banco de origem nunca é limpo.

## Modelo de dados

### Tabelas principais

| Tabela | Descrição |
|--------|-----------|
| `Usuario` | Usuários do sistema (`IsAdmin`) |
| `ContaBancaria` | Contas PF/PJ |
| `Pessoa` | Clientes/parceiros por usuário (`Tipo`: 1=Cliente, 2=Parceiro) |
| `Parceria` | Parcerias (nome + parceiro + cliente + valor + % do parceiro) por usuário |
| `Contrato` | Contratos (nome + cliente + valor, sem parceiro) por usuário |
| `Processo` | Processos (nome + descrição + vínculo **opcional** parceria/contrato, no máximo um via CHECK; `IdModeloProcesso?`, `IdCliente?` direto em `Pessoa`, `DataEncerramento?`) por usuário |
| `ProcessoEtapa` | Fases do processo (nome + descrição + ordem + conclusão + prevista + `DataInicio?` p/ tempo por fase) |
| `ProcessoEtapaItem` | Sub-itens da fase (nome + descrição + `Obrigatorio`/`ExigeAnexo` + ordem + conclusão + `DataInicio`/`DataConclusao` p/ tempo por item) |
| `ProcessoAnexo` | Anexos do item (estrutura p/ Plano 2: `DriveFileId`/`Url`) |
| `ModeloProcesso` / `ModeloEtapa` / `ModeloItem` | Templates reutilizáveis por usuário (fases + itens com `Obrigatorio`/`ExigeAnexo`) — instanciados ao criar processo |
| `CategoriaReceita` / `CategoriaDespesa` / `CategoriaServico` | Categorias (pai/sub) — **compartilhadas** |
| `CategoriaHistorico` | Auditoria de cria/edita/exclui de categorias |
| `Receita` | Receitas (avulsas e recorrentes) — `IdParceria`/`IdContrato` opcionais e mutuamente exclusivos para vínculo |
| `Despesa` | Despesas (avulsas e recorrentes) — `IdReceitaOrigem` e `IdParceria` opcionais para vínculos |
| `PermissaoUsuario` | Nível por módulo por usuário (`dashboard`, `receitas`, `despesas`, `contas`, `categorias`, `clientes`, `parceiros`, `parcerias`, `contratos`, `processos` garantidos via seed; admin com `Escrita` em todos) |

> **Caixa do dashboard** (`DataRealizacao`): o recebido/pago por mês do dashboard usa `COALESCE(DataRealizacao, Data)` quando `Status=2` — o dinheiro conta no mês em que efetivamente entrou/saiu, não no mês do vencimento. Queries em `LancamentoSql` (`ResumoAnualRealizadoPorMes`, `ResumoMensalRealizado`, `*RealizadoPorConta`).

### Exclusão de usuário

- `Usuario` é referenciado por 14 FKs sem `ON DELETE CASCADE` (`ContaBancaria`, `Pessoa`, `Parceria`, `Contrato`, `Processo`, `ModeloProcesso`, `CategoriaReceita/Despesa/Servico`, `RegraReceita/Despesa`, `Receita`, `Despesa`, `CategoriaHistorico`)
- `DELETE /api/usuarios/{id}` sem flag conta vínculos via `UsuarioRepository.ContarVinculosAsync`; se `> 0` retorna `USUARIO_COM_VINCULOS` → 422 orientando desativar
- Exclusão em cascata (`DELETE /{id}?cascata=true&confirmacao=sim`): exige `confirmacao=sim` e executa `UsuarioSql.ExcluirEmCascata` numa única conexão (transação implícita) na ordem segura pelas FKs: `ReceitaServico`/`DespesaServico` → `ProcessoAnexo` → `ProcessoEtapaItem` → `ProcessoEtapa` → `Receita`/`Despesa` → `Processo` → `ModeloItem` → `ModeloEtapa` → `ModeloProcesso` → `Contrato`/`Parceria` → `RegraReceita`/`RegraDespesa` → `ContaBancaria`/`Pessoa` → `CategoriaReceita`/`Despesa`/`Servico` → `CategoriaHistorico` → `PermissaoUsuario` → `Usuario`
- Auto-exclusão é bloqueada (`AUTO_EXCLUSAO` → 422) no backend além do frontend

> ⚠️ Bug conhecido (pré-existente, falha igual no SQL Server e no Postgres): a cascata quebra
> quando o usuário criou categorias de serviço referenciadas por `ReceitaServico`/`DespesaServico`
> de outros usuários (FK `SubcategoriaServicoId`). Além disso, a cascata apaga categorias que são
> **compartilhadas** — risco de perder categorias usadas pelo time.

### Categorias compartilhadas

- Leitura para **todos** os usuários (listagem retorna todas as ativas)
- **Editar/excluir**: apenas o dono (`IdUsuario`) ou usuário `IsAdmin` → senão HTTP 403 (`Erro.Permissao`)
- Toda mutação (criar/editar/excluir, incluindo subcategorias) grava `CategoriaHistorico`
  - `Acao`: 1=Criado, 2=Editado, 3=Excluído
  - `TipoCategoria`: 1=Receita, 2=Despesa, 3=Serviços

## PostgreSQL local (dev)

Réplica PostgreSQL em `D:\projetos\postgres-replica` (fora do repositório):

- Binários: `pgsql\bin\` · dados: `data\` · porta `5432`
- Auto-start no logon via `PortalFinanceiroPostgres.vbs` (pasta Startup do Windows)
- Scripts manuais: `iniciar.bat` / `parar.bat`
- Usuário `postgres`, senha vazia (auth trust local), banco `portal_financeiro`