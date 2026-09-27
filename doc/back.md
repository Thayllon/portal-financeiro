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
| `/api/auth/alterar-senha` | POST | Alterar a própria senha |
| `/api/receitas` | GET/POST/PUT/DELETE | Lançamentos de receita |
| `/api/receitas/{id}/receber` | POST | Marcar como recebido |
| `/api/receitas/{id}/estornar` | POST | Estornar recebimento |
| `/api/despesas` | GET/POST/PUT/DELETE | Lançamentos de despesa |
| `/api/despesas/{id}/pagar` | POST | Marcar como pago |
| `/api/despesas/{id}/estornar` | POST | Estornar pagamento |
| `/api/regras-receitas` | GET/PUT/DELETE | Regras recorrentes de receita |
| `/api/regras-despesas` | GET/PUT/DELETE | Regras recorrentes de despesa |
| `/api/contas-bancarias` | GET/POST/PUT/DELETE | Contas bancárias |
| `/api/contas-bancarias/{id}/padrao` | PUT | Definir conta padrão do usuário |
| `/api/pessoas` | GET/POST/PUT/DELETE | Clientes/parceiros (`Tipo`: 1=Cliente, 2=Parceiro) |
| `/api/parcerias` | GET (`ativo?`)/POST/PUT/DELETE · PUT /{id}/encerrar\|reativar | Parcerias (nome + parceiro + cliente + valor + % do parceiro) — `ValorParceiro`/`MinhaParte` calculados; saldo via `TotalRecebido/Pago` e `FaltaReceber/Pagar` (a receber sobre o valor cheio, a pagar sobre a parte do parceiro); encerrar exige falta receber e falta pagar zerados (`PARCERIA_COM_PENDENCIAS` → 422) |
| `/api/parcerias/{id}/receitas` | GET | Receitas vinculadas à parceria (dono validado) |
| `/api/parcerias/{id}/despesas` | GET | Despesas vinculadas à parceria (dono validado) |
| `/api/parcerias/resumo` | GET (`ano`, `mes`) | Resumo do mês (recebido/pago/a receber/a pagar + qtd) |
| `/api/contratos` | GET (`ativo?`)/POST/PUT/DELETE · PUT /{id}/encerrar\|reativar | Contratos (nome + cliente + valor, sem parceiro) — saldo via `TotalRecebido` e `FaltaReceber`; encerrar exige falta receber zerada (`CONTRATO_COM_PENDENCIAS` → 422); receita vinculada via `IdContrato` (no máximo um vínculo por receita: parceria ou contrato) |
| `/api/contratos/{id}/receitas` | GET | Receitas vinculadas ao contrato (dono validado) |
| `/api/processos` | GET (`ativo?`)/POST/PUT/DELETE · PUT /{id}/encerrar\|reativar | Processos (nome + descrição + vínculo obrigatório e exclusivo: `IdParceria` XOR `IdContrato`, imutável após criar) — progresso via `TotalEtapas`/`EtapasConcluidas`/`PercentualConcluido`; encerrar exige 100% das etapas concluídas (`PROCESSO_COM_ETAPAS_PENDENTES` → 422); excluir é física (remove o processo e as etapas, some de todos os filtros); excluir parceria/contrato com processo ativo vinculado é bloqueado (`PARCERIA_COM_PROCESSOS`/`CONTRATO_COM_PROCESSOS` → 422) |
| `/api/processos/{id}/etapas` | POST | Criar etapa (nome + descrição + `DataPrevista?`; ordem automática sequencial) |
| `/api/processos/{id}/etapas/{etapaId}` | PUT/DELETE | Editar (nome/descrição/prevista) ou excluir etapa |
| `/api/processos/{id}/etapas/{etapaId}/concluir\|estornar` | PUT | Marcar conclusão (preenche `DataConclusao`) ou estornar (limpa) |
| `/api/processos/{id}/etapas/{etapaId}/mover` | PUT (`direcao` -1\|1) | Reordenar etapa (troca ordem com a vizinha) |
| `/api/categorias/receita` | GET/POST/PUT/DELETE | Categorias de receita (compartilhadas) |
| `/api/categorias/despesa` | GET/POST/PUT/DELETE | Categorias de despesa (compartilhadas) |
| `/api/categorias/servicos` | GET/POST/PUT/DELETE | Categorias de serviços (compartilhadas) |
| `/api/usuarios` | GET/POST/PUT/DELETE · PATCH /{id}/ativo · PATCH /{id}/senha | Gerenciamento de usuários (somente admin). Excluir valida auto-exclusão (`AUTO_EXCLUSAO` → 422) e vínculos (`USUARIO_COM_VINCULOS` → 422, contagem em 13 tabelas); com vínculos, desativar em vez de excluir |
| `/api/usuarios/{usuarioId}/permissoes` | GET/PUT | Permissões por módulo do usuário (somente admin) |
| `/api/diagnostico` | GET (somente admin) | Diagnóstico QA ao vivo: regras R1–R6 de `doc/regras.md` com cenários executados, saúde do banco (leitura) e débitos técnicos. Sem escrita. Alimenta a tela `/testes` |
| `/api/dashboard` | GET | Dashboard mensal (`mes`, `ano`, `idConta?` Guid): realizado + previsto do mês (`totalReceitasPrevisto/totalDespesasPrevisto/saldoPrevisto` via regras vigentes — filtradas por conta quando `idConta` — descontando o já materializado por `IdRegra`, inclusive por conta em `resumoPorConta`) e `resumoParcerias` do mês (pago/recebido/a pagar/a receber + qtd) |
| `/api/dashboard/anual` | GET | Dashboard anual (`ano`, `idConta?` Guid): totais + variação % vs ano anterior, média mensal pró-rata, `totalReceitasRecorrentes/totalDespesasRecorrentes` (soma de `ResumoAnualPorMes.TotalRecorrente`, onde `IdRegra IS NOT NULL`), `resumoPorMes[12]` com `saldoAcumulado` mês a mês, `resumoPorConta` (respeita `idConta`), `distribuicaoReceitas/Despesas` por categoria/subcategoria com %, `previsaoRestanteAno` (meses restantes via regras recorrentes) e `resumoParcerias` (recebido/pago/a receber/a pagar do ano + qtd) |

**Autorização por posse:** operações por `{id}` (Obter, Atualizar, Excluir e marcar/estornar de receitas/despesas) validam que o recurso pertence ao usuário autenticado (via `IdUsuario` do registro). Recurso de outro usuário retorna `Erro.Permissao` → **HTTP 403**. Categorias compartilhadas: editar/excluir somente o dono ou admin → 403.

**Autorização por nível de permissão (módulo):** endpoints de escrita (POST/PUT/DELETE e ações como receber/estornar/pagar/encerrar/reativar/definir padrão) exigem o nível **Escrita** no módulo correspondente da tabela `PermissaoUsuario` (0=Nenhum, 1=Leitura, 2=Escrita). A verificação é feita pelo atributo `[RequerPermissaoEscrita("<modulo>")]` (`PortalFinanceiro.API/Authorization/`), que consulta `IPermissaoUsuarioAppService.VerificarPermissaoAsync`; sem Escrita retorna `Erro.Permissao("PERMISSAO_ESCRITA_NEGADA", ...)` → **HTTP 403**. Admin (`IsAdmin` / role `Admin`) passa direto. Mapeamento de módulos: `receitas`/`despesas` (incl. regras recorrentes), `contas`, `categorias` (receita/despesa/serviços), `parcerias`, `contratos`, `processos`. Pessoas usam `[RequerPermissaoEscritaPessoa]`, que resolve o módulo pelo `TipoPessoa` do body (`clientes` ou `parceiros`).

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