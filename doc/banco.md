# Banco de dados — Portal Financeiro

## Providers

| Provider | Pasta | Status |
|----------|-------|--------|
| SQL Server | `scripts/sqlserver/` | **Padrão do app** (LocalDB) |
| PostgreSQL | `scripts/postgres/` | Pronto (uuid, boolean, `gen_random_uuid`), aguarda suporte no `SqlDialect` |

## Scripts

Cada provider tem o **mesmo conjunto "from scratch"** (banco novo):

| Script | Conteúdo |
|--------|----------|
| `001_CriarTabelas.sql` | Schema unificado completo (todas as tabelas, índices, FKs — inclui `Pessoa`, `CategoriaServico`, `ReceitaServico`, `DespesaServico` + `Despesa.IdCliente`, `Parceria` com `Nome`/`PercentualParceiro` e `PermissaoUsuario`) |
| `003_FluxoAdicionalDespesa.sql` | Incremental idempotente para bancos criados antes do refactor: adiciona `Despesa.IdCliente` e tabela `DespesaServico` se ainda não existirem |
| `099_SeedBase.sql` | Admin + garantia do módulo `parcerias` para usuários sem a permissão |

> **"From scratch"** = executar somente em banco novo. Um banco de desenvolvimento já
> migrado **não** deve recebê-los novamente (DbUp rastreia por nome).
>
> Os incrementais antigos (`006`–`014` no SQL Server, `002`–`006` no Postgres e
> `002_AdicionarNomePercentualParceria`) foram removidos por já estarem absorvidos no `001` — com exceção do backfill de
> `parcerias`, movido para o `099_SeedBase`. O `003_FluxoAdicionalDespesa` foi **recriado** idempotente pois o `001` from-scratch não é reaplicado em bancos existentes e o erro “Não foi possível retornar as despesas” ocorria justamente pela falta de `IdCliente`/`DespesaServico`.

### Differs entre providers

- **SQL Server**: `IDENTITY`, `BIT`, tabela em schema `dbo`
- **PostgreSQL**: `SERIAL`/`IDENTITY`, `BOOLEAN`, UUID via `gen_random_uuid()`

## Ferramenta: DbSetup

`tools/DbSetup` aplica os scripts via DbUp.

```bash
# Criar banco padrão (SQL Server LocalDB) + rodar scripts de scripts/sqlserver
dotnet run --project tools/DbSetup

# Pasta customizada (ex.: Postgres)
dotnet run --project tools/DbSetup -- --scripts=C:\caminho\scripts\postgres
```

- Cria o banco `PortalFinanceiro` no LocalDB caso não exista
- Roda os scripts em ordem de nome (não aplica os que já estão no journal)

## Modelo de dados

### Tabelas principais

| Tabela | Descrição |
|--------|-----------|
| `Usuario` | Usuários do sistema (`IsAdmin`) |
| `ContaBancaria` | Contas PF/PJ |
| `Pessoa` | Clientes/parceiros por usuário (`Tipo`: 1=Cliente, 2=Parceiro) |
| `Parceria` | Parcerias (nome + parceiro + cliente + valor + % do parceiro) por usuário |
| `CategoriaReceita` / `CategoriaDespesa` / `CategoriaServico` | Categorias (pai/sub) — **compartilhadas** |
| `CategoriaHistorico` | Auditoria de cria/edita/exclui de categorias |
| `Receita` | Receitas (avulsas e recorrentes) — `IdParceria` opcional para vínculo com Parceria |
| `Despesa` | Despesas (avulsas e recorrentes) — `IdReceitaOrigem` e `IdParceria` opcionais para vínculos |
| `PermissaoUsuario` | Nível por módulo por usuário (`parcerias` garantido via seed) |
| `RegraReceita` / `RegraDespesa` | Recorrências mensais (fixas/variáveis) |

### Categorias compartilhadas

- Leitura para **todos** os usuários (listagem retorna todas as ativas)
- **Editar/excluir**: apenas o dono (`IdUsuario`) ou usuário `IsAdmin` → senão HTTP 403 (`Erro.Permissao`)
- Toda mutação (criar/editar/excluir, incluindo subcategorias) grava `CategoriaHistorico`
  - `Acao`: 1=Criado, 2=Editado, 3=Excluído
  - `TipoCategoria`: 1=Receita, 2=Despesa, 3=Serviços
