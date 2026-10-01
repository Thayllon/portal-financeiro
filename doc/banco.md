# Banco de dados â€” Portal Financeiro

## Providers

| Provider | Pasta | Status |
|----------|-------|--------|
| SQL Server | `scripts/sqlserver/` | **PadrÃ£o do app** (LocalDB) |
| PostgreSQL | `scripts/postgres/` | Suportado (`Npgsql` + `PostgresDialect`; provider via `Database__Provider`) |

## Scripts

Ambos os providers tÃªm o **mesmo conjunto "from scratch"** (banco novo) mais
incrementais idempotentes para bancos jÃ¡ criados:

| Script | ConteÃºdo |
|--------|----------|
| `001_CriarTabelas.sql` | Schema unificado completo (todas as tabelas, Ã­ndices, FKs â€” inclui `Pessoa`, `CategoriaServico`, `ReceitaServico`, `DespesaServico` + `Despesa.IdCliente`, `Parceria` com `Nome`/`PercentualParceiro`, `Contrato` + `Receita.IdContrato`, `Processo` + `ProcessoEtapa` e `PermissaoUsuario`) |
| `003_FluxoAdicionalDespesa.sql` | Incremental idempotente para bancos criados antes do refactor: adiciona `Despesa.IdCliente` e tabela `DespesaServico` se ainda nÃ£o existirem |
| `004_ContaPadrao.sql` | Incremental idempotente: adiciona `ContaBancaria.EhPadrao` se ainda nÃ£o existir |
| `005_Contratos.sql` | Incremental idempotente: cria `Contrato`, adiciona `Receita.IdContrato` (FK + Ã­ndice) e garante o mÃ³dulo `contratos` em `PermissaoUsuario` |
| `006_ContratoRecorrente.sql` | Incremental idempotente: adiciona `Contrato.EhRecorrente` + `Contrato.IdRegra` se ainda nÃ£o existirem |
| `015_Processos.sql` | Incremental idempotente: cria `Processo` (vÃ­nculo opcional `IdParceria`/`IdContrato` â€” no mÃ¡ximo um via CHECK) + `ProcessoEtapa` (ordem, conclusÃ£o, prevista) e garante o mÃ³dulo `processos` em `PermissaoUsuario` |
| `016_OutrosIndicadores.sql` | Incremental idempotente: garante o mÃ³dulo especial `outros-indicadores` (Leitura) para admin e quem jÃ¡ usa fluxo adicional |
| `017_ProcessoVinculoOpcional.sql` | Incremental idempotente: troca o CHECK do `Processo` â€” de "vÃ­nculo obrigatÃ³rio e exclusivo (XOR)" para "no mÃ¡ximo um" (processo pode ser criado sem vÃ­nculo) |
| `018_CheckEnums.sql` | Incremental idempotente: `CHECK constraints` dos enums (tipo/nÃ­vel/status) |
| `019_PermissaoCategoriasGranulares.sql` | Incremental: divide `categorias` em 3 mÃ³dulos granulares preservando nÃ­veis |
| `020_AuditoriaAtor.sql` | Incremental idempotente: adicionar `CriadoPor`/`AlteradoPor` nas tabelas de domÃ­nio |
### Operacionais Postgres (sÃ³ em `scripts/postgres/` â€” uso manual, NÃƒO via DbUp)

| Script | ConteÃºdo |
|--------|----------|
| `100_DDL_AtualizarEstrutura.sql` | **DDL idempotente p/ Neon desatualizado**: sincroniza schema (cria tabelas/colunas/Ã­ndices/FKs faltantes) |
| `103_CheckEnums.sql` | DDL: `CHECK constraints` dos enums (tipo/nÃ­vel/status) |
| `104_PermissaoCategoriasGranulares.sql` | DML de permissÃµes: divide `categorias` em los 3 mÃ³dulos granulares preservando nÃ­veis |
| `105_AuditoriaAtor.sql` | DDL: adicionar `CriadoPor`/`AlteradoPor` (auditoinÃ­cio no futuro, dados antigos ficam com NULL) |
### Estrutura Postgres

> O repositÃ³rio mantÃ©m **somente DDL + scripts de permissÃµes**. Scripts de dados/remise (seeds DML destrutivos) foram removidos; cargas de dados sÃ£o executadas pontualmente no banco, via SQL Editor/psql, e nÃ£o versionadas.

> **Copiar Local → Prod (Neon):** fluxo descontinuado. Estrutura via `100_DDL_AtualizarEstrutura.sql`
> (+ `103`/`104`/`105`); dados sÃ£o carga pontual, nÃ£o versionada.

> **"From scratch"** = executar somente em banco novo. Um banco de desenvolvimento jÃ¡
> migrado **nÃ£o** deve recebÃª-los novamente (DbUp rastreia por nome).
>
> Os incrementais antigos (`006`â€“`014` no SQL Server, `002`â€“`006` no Postgres e
> `002_AdicionarNomePercentualParceria`) foram removidos por jÃ¡ estarem absorvidos no `001` â€” com exceÃ§Ã£o do backfill de
> `parcerias`. A numeraÃ§Ã£o foi **reutilizada**: os atuais
> `003`/`004`/`005`/`006` sÃ£o scripts novos e idempotentes para bancos jÃ¡ criados (o `003_FluxoAdicionalDespesa` foi
> **recriado** idempotente pois o `001` from-scratch nÃ£o Ã© reaplicado em bancos existentes e o erro â€œNÃ£o foi possÃ­vel retornar as despesasâ€ ocorria justamente pela falta de `IdCliente`/`DespesaServico`).

### Differs entre providers

- **SQL Server**: `IDENTITY`, `BIT`, tabela em schema `dbo`
- **PostgreSQL**: `SERIAL`/`IDENTITY`, `BOOLEAN`, UUID via `gen_random_uuid()`

## Ferramenta: DbSetup

`tools/DbSetup` aplica os scripts via DbUp.

```bash
# Criar banco padrÃ£o (SQL Server LocalDB) + rodar scripts de scripts/sqlserver
dotnet run --project tools/DbSetup
```

> O `DbSetup` Ã© **somente SQL Server** (DbUp + `Microsoft.Data.SqlClient`) e roda
> todos os scripts da pasta em ordem de nome. **NÃ£o** aponte `--scripts=` para
> `scripts/postgres/` (sintaxe Postgres nÃ£o roda no LocalDB).

- Cria o banco `PortalFinanceiro` no LocalDB caso nÃ£o exista
- Roda os scripts em ordem de nome (nÃ£o aplica os que jÃ¡ estÃ£o no journal)

## Modelo de dados

### Tabelas principais

| Tabela | DescriÃ§Ã£o |
|--------|-----------|
| `Usuario` | UsuÃ¡rios do sistema (`IsAdmin`) |
| `ContaBancaria` | Contas PF/PJ |
| `Pessoa` | Clientes/parceiros por usuÃ¡rio (`Tipo`: 1=Cliente, 2=Parceiro) |
| `Parceria` | Parcerias (nome + parceiro + cliente + valor + % do parceiro) por usuÃ¡rio |
| `Contrato` | Contratos (nome + cliente + valor, sem parceiro) por usuÃ¡rio |
| `Processo` | Processos (nome + descriÃ§Ã£o + vÃ­nculo **opcional** parceria/contrato, no mÃ¡ximo um via CHECK) por usuÃ¡rio |
| `ProcessoEtapa` | Etapas do processo (nome + descriÃ§Ã£o + ordem + conclusÃ£o + prevista) |
| `CategoriaReceita` / `CategoriaDespesa` / `CategoriaServico` | Categorias (pai/sub) â€” **compartilhadas** |
| `CategoriaHistorico` | Auditoria de cria/edita/exclui de categorias |
| `Receita` | Receitas (avulsas e recorrentes) â€” `IdParceria`/`IdContrato` opcionais e mutuamente exclusivos para vÃ­nculo |
| `Despesa` | Despesas (avulsas e recorrentes) â€” `IdReceitaOrigem` e `IdParceria` opcionais para vÃ­nculos |
| `PermissaoUsuario` | NÃ­vel por mÃ³dulo por usuÃ¡rio (`dashboard`, `receitas`, `despesas`, `contas`, `categorias`, `clientes`, `parceiros`, `parcerias`, `contratos`, `processos` garantidos via seed; admin com `Escrita` em todos) |

> **Caixa do dashboard** (`DataRealizacao`): o recebido/pago por mÃªs do dashboard usa `COALESCE(DataRealizacao, Data)` quando `Status=2` â€” o dinheiro conta no mÃªs em que efetivamente entrou/saiu, nÃ£o no mÃªs do vencimento. Queries em `LancamentoSql` (`ResumoAnualRealizadoPorMes`, `ResumoMensalRealizado`, `*RealizadoPorConta`).

### ExclusÃ£o de usuÃ¡rio

- `Usuario` Ã© referenciado por 13 FKs sem `ON DELETE CASCADE` (`ContaBancaria`, `Pessoa`, `Parceria`, `Contrato`, `Processo`, `CategoriaReceita/Despesa/Servico`, `RegraReceita/Despesa`, `Receita`, `Despesa`, `CategoriaHistorico`)
- `DELETE /api/usuarios/{id}` sem flag conta vÃ­nculos via `UsuarioRepository.ContarVinculosAsync`; se `> 0` retorna `USUARIO_COM_VINCULOS` â†’ 422 orientando desativar
- ExclusÃ£o em cascata (`DELETE /{id}?cascata=true&confirmacao=sim`): exige `confirmacao=sim` e executa `UsuarioSql.ExcluirEmCascata` numa Ãºnica conexÃ£o (transaÃ§Ã£o implÃ­cita) na ordem segura pelas FKs: `ReceitaServico`/`DespesaServico` â†’ `ProcessoEtapa` â†’ `Receita`/`Despesa` â†’ `Processo` â†’ `Contrato`/`Parceria` â†’ `RegraReceita`/`RegraDespesa` â†’ `ContaBancaria`/`Pessoa` â†’ `CategoriaReceita`/`Despesa`/`Servico` â†’ `CategoriaHistorico` â†’ `PermissaoUsuario` â†’ `Usuario`
- Auto-exclusÃ£o Ã© bloqueada (`AUTO_EXCLUSAO` â†’ 422) no backend alÃ©m do frontend
| `RegraReceita` / `RegraDespesa` | RecorrÃªncias mensais (fixas/variÃ¡veis) |

### Categorias compartilhadas

- Leitura para **todos** os usuÃ¡rios (listagem retorna todas as ativas)
- **Editar/excluir**: apenas o dono (`IdUsuario`) ou usuÃ¡rio `IsAdmin` â†’ senÃ£o HTTP 403 (`Erro.Permissao`)
- Toda mutaÃ§Ã£o (criar/editar/excluir, incluindo subcategorias) grava `CategoriaHistorico`
  - `Acao`: 1=Criado, 2=Editado, 3=ExcluÃ­do
  - `TipoCategoria`: 1=Receita, 2=Despesa, 3=ServiÃ§os
