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
dotnet run --project src/PortalFinanceiro.API
```

> A API precisa do banco criado primeiro — veja [primeiros-passos.md](primeiros-passos.md).

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
| `/api/categorias/receita` | GET/POST/PUT/DELETE | Categorias de receita (compartilhadas) |
| `/api/categorias/despesa` | GET/POST/PUT/DELETE | Categorias de despesa (compartilhadas) |
| `/api/usuarios` | GET/POST/PUT · PATCH /{id}/ativo | Gerenciamento de usuários (somente admin) |
| `/api/dashboard` | GET | Dashboard com resumo |

## Padrões de código

Regras completas no [AGENTS.md](../AGENTS.md). Resumo:

- Entidades usam `private set`; propriedades de navegação (string display) vão em DTO/projeção
- Services retornam `Result<T>` (result pattern), nunca exceptions
- `idUsuario` vem de `User.FindFirst(ClaimTypes.NameIdentifier)` (JWT), nunca de query param
- Controllers só chamam service + `ApiResponse`; sem lógica de negócio
- Categorias compartilhadas: editar/excluir só dono ou admin → senão `Erro.Permissao` (HTTP 403)
- Auditoria de categorias grava `CategoriaHistorico` em toda mutação
- Encargos DAS via `IEncargoFiscalService`, vinculados por `Despesa.IdReceitaOrigem`
- Constantes fiscais (% DAS, nomes CNPJ/DAS) em `EncargoFiscal` (`Domain/Services/`)
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

## Encargos automáticos

| Encargo | Origem | % padrão | Categoria | Vínculo |
|---------|--------|----------|-----------|---------|
| **DAS** | Receita com flag "nota fiscal" (avulsa ou parcela recorrente) | 6% (editável) | CNPJ → DAS | `Despesa.IdReceitaOrigem` |

- A despesa gerada usa a **mesma conta** da origem e entra na lista de despesas como lançamento normal (pode pagar/estornar)
- Criar/editar/excluir a receita **sincroniza** a despesa de encargo
- Falha ao gerar encargo desfaz a criação da receita (transação)