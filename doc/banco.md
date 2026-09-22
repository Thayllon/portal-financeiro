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
| `001_CriarTabelas.sql` | Schema unificado completo (todas as tabelas, índices, FKs — inclui `Pessoa`, `CategoriaServico`, `ReceitaServico`, `DespesaServico` + `Despesa.IdCliente`, `Parceria` com `Nome`/`PercentualParceiro`, `Contrato` + `Receita.IdContrato` e `PermissaoUsuario`) |
| `003_FluxoAdicionalDespesa.sql` | Incremental idempotente para bancos criados antes do refactor: adiciona `Despesa.IdCliente` e tabela `DespesaServico` se ainda não existirem |
| `005_Contratos.sql` | Incremental idempotente: cria `Contrato`, adiciona `Receita.IdContrato` (FK + índice) e garante o módulo `contratos` em `PermissaoUsuario` |
| `099_SeedBase.sql` | Admin + garantia do módulo `parcerias` para usuários sem a permissão |
| `100_SeedDemo.sql` | Dados fake de demonstração (Maria/João) — 2024-01 a 2026-09 |
| `101_AtualizarUsuariosDemo.sql` | Troca e-mail/senha dos usuários demo para produção: `maria@portal.com` / `joao@portal.com`, senha `123456` (idempotente) |
| `102_DDL_AtualizarEstrutura.sql` | **DDL idempotente p/ Neon desatualizado**: sincroniza schema (cria tabelas/colunas/índices/FKs faltantes) |
| `103_DML_Limpar_E_Copiar_Dados.sql` | **DML idempotente**: `TRUNCATE CASCADE` — esvazia o banco (rodar antes de copiar) |

> **Copiar Local → Prod (Neon):** DDL e DML separados, conforme solicitado:
> 1. **DDL — estrutura:** `psql "$DATABASE_URL" -f scripts/postgres/102_DDL_AtualizarEstrutura.sql`
> 2. **DML — dados:** gere o arquivo com seu LocalDB: `powershell -ExecutionPolicy Bypass -File scripts/CopiarLocalParaProd.ps1`
>    → cria `portal-financeiro-prod-restore.sql` na **raiz do projeto** (o Explorer abre sozinho).
>    Depois: `psql "$DATABASE_URL" -f portal-financeiro-prod-restore.sql` ou cole no SQL Editor do Neon.
>    O gerado já contém `TRUNCATE CASCADE` + `INSERTs` convertidos p/ Postgres (`::uuid`, `TRUE`/`FALSE`).
>    Alternativa rápida: `psql "$DATABASE_URL" -f scripts/postgres/103_DML_Limpar_E_Copiar_Dados.sql` só para esvaziar.

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
| `Contrato` | Contratos (nome + cliente + valor, sem parceiro) por usuário |
| `CategoriaReceita` / `CategoriaDespesa` / `CategoriaServico` | Categorias (pai/sub) — **compartilhadas** |
| `CategoriaHistorico` | Auditoria de cria/edita/exclui de categorias |
| `Receita` | Receitas (avulsas e recorrentes) — `IdParceria`/`IdContrato` opcionais e mutuamente exclusivos para vínculo |
| `Despesa` | Despesas (avulsas e recorrentes) — `IdReceitaOrigem` e `IdParceria` opcionais para vínculos |
| `PermissaoUsuario` | Nível por módulo por usuário (`dashboard`, `receitas`, `despesas`, `contas`, `categorias`, `clientes`, `parceiros`, `parcerias`, `contratos` garantidos via seed; admin com `Escrita` em todos) |

### Exclusão de usuário

- `Usuario` é referenciado por 12 FKs sem `ON DELETE CASCADE` (`ContaBancaria`, `Pessoa`, `Parceria`, `Contrato`, `CategoriaReceita/Despesa/Servico`, `RegraReceita/Despesa`, `Receita`, `Despesa`, `CategoriaHistorico`)
- `DELETE /api/usuarios/{id}` conta vínculos via `UsuarioRepository.ContarVinculosAsync`; se `> 0` retorna `USUARIO_COM_VINCULOS` → 422 com mensagem orientando desativar em vez de excluir
- Auto-exclusão é bloqueada (`AUTO_EXCLUSAO` → 422) no backend além do frontend
| `RegraReceita` / `RegraDespesa` | Recorrências mensais (fixas/variáveis) |

### Categorias compartilhadas

- Leitura para **todos** os usuários (listagem retorna todas as ativas)
- **Editar/excluir**: apenas o dono (`IdUsuario`) ou usuário `IsAdmin` → senão HTTP 403 (`Erro.Permissao`)
- Toda mutação (criar/editar/excluir, incluindo subcategorias) grava `CategoriaHistorico`
  - `Acao`: 1=Criado, 2=Editado, 3=Excluído
  - `TipoCategoria`: 1=Receita, 2=Despesa, 3=Serviços
