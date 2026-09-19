# Backend — Portal Financeiro

## Stack

- **.NET 11** + ASP.NET Core (API REST)
- **Clean Architecture**: `API` (controllers) → `Core` (domínio + aplicação) → `Infrastructure` (Dapper + IoC)
- **Dapper** como ORM + **Polly** para retry
- **JWT Bearer** para autenticação
- **DbUp** para migrations (via `tools/DbSetup`, ver [banco.md](banco.md))

## Projetos

| Projeto | Função |
|---------|--------|
| `PortalFinanceiro.API` | Controllers, middleware, DI, startup (`Program.cs`) |
| `PortalFinanceiro.Core` | Domain entities, DTOs, services, interfaces |
| `PortalFinanceiro.Infrastructure` | Dapper repositories, IoC |

## Como rodar / buildar / testar

```bash
# Build
dotnet build PortalFinanceiro.API.slnx

# Rodar API (http://localhost:5178, Swagger em /swagger)
$env:Auth__Secret = "um-segredo-aleatorio-forte"   # obrigatório (JWT), ver .env.example
dotnet run --project src/PortalFinanceiro.API
```

> A API precisa do banco criado primeiro — veja [primeiros-passos.md](primeiros-passos.md).
> A chave JWT não é versionada: vem da variável de ambiente `Auth__Secret` (config `Auth:Secret`). Em docker, o `docker-compose*.yml` injeta a partir de `JWT_SECRET`. A aplicação falha ao iniciar se a chave estiver vazia.

## Rotas da API

| Rota | Método | Descrição |
|------|--------|-----------|
| `/api/auth/login` | POST | Login |
| `/api/auth/token` | GET | Token do admin de desenvolvimento |
| `/api/receitas` | GET/POST/PUT/DELETE | Lançamentos de receita |
| `/api/receitas/{id}/receber` | POST | Marcar como recebido |
| `/api/receitas/{id}/estornar` | POST | Estornar recebimento |
| `/api/despesas` | GET/POST/PUT/DELETE | Lançamentos de despesa |
| `/api/despesas/{id}/pagar` | POST | Marcar como pago |
| `/api/despesas/{id}/estornar` | POST | Estornar pagamento |
| `/api/regras-receitas` | GET/PUT/DELETE | Regras recorrentes de receita |
| `/api/regras-despesas` | GET/PUT/DELETE | Regras recorrentes de despesa |
| `/api/contas-bancarias` | GET/POST/PUT/DELETE | Contas bancárias |
| `/api/pessoas` | GET/POST/PUT/DELETE | Clientes/parceiros (`Tipo`: 1=Cliente, 2=Parceiro) |
| `/api/parcerias` | GET/POST/PUT/DELETE | Parcerias (nome + parceiro + cliente + valor + % do parceiro) — `ValorParceiro`/`MinhaParte` calculados; saldo via `TotalRecebido/Pago` e `FaltaReceber/Pagar` (a receber sobre o valor cheio, a pagar sobre a parte do parceiro) |
| `/api/parcerias/{id}/receitas` | GET | Receitas vinculadas à parceria (dono validado) |
| `/api/parcerias/{id}/despesas` | GET | Despesas vinculadas à parceria (dono validado) |
| `/api/categorias/receita` | GET/POST/PUT/DELETE | Categorias de receita (compartilhadas) |
| `/api/categorias/despesa` | GET/POST/PUT/DELETE | Categorias de despesa (compartilhadas) |
| `/api/categorias/servicos` | GET/POST/PUT/DELETE | Categorias de serviços (compartilhadas) |
| `/api/usuarios` | GET/POST/PUT · PATCH /{id}/ativo | Gerenciamento de usuários (somente admin) |
| `/api/dashboard` | GET | Dashboard mensal (`mes`, `ano`, `idConta?` Guid): realizado + previsto do mês (`totalReceitasPrevisto/totalDespesasPrevisto/saldoPrevisto` via regras vigentes — filtradas por conta quando `idConta` — descontando o já materializado por `IdRegra`, inclusive por conta em `resumoPorConta`) e `resumoParcerias` do mês (pago/recebido/a pagar/a receber + qtd) |
| `/api/dashboard/anual` | GET | Dashboard anual (`ano`, `idConta?` Guid): totais + variação % vs ano anterior, média mensal pró-rata, `resumoPorMes[12]`, `resumoPorConta` (respeita `idConta`), `distribuicaoReceitas/Despesas` por categoria/subcategoria com %, `previsaoRestanteAno` (meses restantes via regras recorrentes) e `resumoParcerias` (recebido/pago/a receber/a pagar do ano + qtd) |

**Autorização por posse:** operações por `{id}` (Obter, Atualizar, Excluir e marcar/estornar de receitas/despesas) validam que o recurso pertence ao usuário autenticado (via `IdUsuario` do registro). Recurso de outro usuário retorna `Erro.Permissao` → **HTTP 403**. Categorias compartilhadas: editar/excluir somente o dono ou admin → 403.

## Padrões de código

Regras completas no [AGENTS.md](../AGENTS.md). Resumo:

- Entidades usam `private set`; propriedades de navegação (string display) vão em DTO/projeção
- Services retornam `Result<T>` (result pattern), nunca exceptions
- `idUsuario` vem de `User.FindFirst(ClaimTypes.NameIdentifier)` (JWT), nunca de query param
- Recursos por `{id}` validam posse (`Erro.Permissao` → 403) contra o `IdUsuario` do registro
- Controllers só chamam service + `ApiResponse`; sem lógica de negócio
- Categorias compartilhadas: editar/excluir só dono ou admin → senão `Erro.Permissao` (HTTP 403)
- Auditoria de categorias grava `CategoriaHistorico` em toda mutação
- Leitura com projeção (`Domain/Projections/`) para nomes display; entidades têm `private set`
- Após mutação, re-buscar projeção via `ObterProjecaoPorIdAsync` para mapear resposta
- Fluxo recorrente usa `TransactionScope` para atomicidade regra + parcelas
- Status é `int?` (1=Pendente, 2=Realizado), nunca string

## Contrato de erro (resposta)

Toda falha de negócio/validação retorna `Erro` serializado em **camelCase**:

```json
{ "codigo": "RECEITA_JA_RECEBIDA", "mensagem": "Não é possível excluir uma receita já recebida. Estorne primeiro.", "tipo": "Negocio" }
```

- `tipo` (enum `ETipoErro`): `Validacao`, `Negocio`, `NaoEncontrado`, `Conflito`, `Permissao`, `Timeout`, `Externo`, `Infraestrutura` → mapeia para o HTTP status (ex.: `Negocio`=422, `NaoEncontrado`=404, `Permissao`=403).
- `codigo`: código de negócio legível por máquina (ver `Erro.cs`).
- `mensagem`: texto amigável exibido ao usuário (frontend lê `mensagem`/`codigo` em `api-error.util.ts:mensagemErro`).
- Dois caminhos geram esse envelope: `BaseController.ApiResponse` (erros de `Result<T>`) e `ErrorHandlingMiddleware` (exceções não tratadas). Ambos usam camelCase — manter consistente.