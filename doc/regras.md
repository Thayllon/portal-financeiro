# Regras de negócio — Portal Financeiro (wiki micro)

Fonte única das regras críticas. A tela `/testes` (admin) executa cenários
contra estas regras e exibe o macro (passou/quebrou). Cada regra lista onde
vive no código e quais cenários a cobrem. Regra sem cenário = débito visível.

Convenção de cenários: `C<regra>.<n>` (ex.: `C1.2`). Os mesmos cenários existem
em dois lugares: `test/` (xUnit, suite completa) e `IDiagnosticoAppService`
(execução ao vivo em `/api/diagnostico`, sem escrever no banco).

## R1 — Contrato simples

Contrato avulso, sem parceria e sem recorrência. Pode ter N receitas
vinculadas para compor o valor. Encerrar exige falta receber zerada;
excluir com receitas vinculadas é bloqueado.

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Domain/Entities/Contrato.cs:20` (`Criar`), `Core/Application/Services/ContratoAppService.cs:144` (`EncerrarAsync`), `:178` (`ExcluirAsync`) |
| Erros | `NOME_OBRIGATORIO`, `CLIENTE_OBRIGATORIO`, `VALOR_INVALIDO` (400); `CONTRATO_COM_PENDENCIAS`, `CONTRATO_COM_VINCULOS` (422); `CONTRATO_ACESSO_NEGADO` (403) |
| Cenários | `C1.1` criar sem nome → 400 `NOME_OBRIGATORIO`; `C1.2` criar com valor zero → 400 `VALOR_INVALIDO`; `C1.3` encerrar com falta receber > 0 → 422 `CONTRATO_COM_PENDENCIAS` (lógica de serviço, cobertura parcial no diagnóstico) |

## R2 — Contrato recorrente

Contrato que repete todo mês. Ao criar, gera `RegraReceita` + N `Receita`
(`IdContrato` + `IdRegra`) em `TransactionScope` atômico. Exige categoria,
conta, dia e data fim. Dia útil limita a 1–5. Receitas usam a subcategoria
`contrato recorrente` quando existir.

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Application/Dtos/Request/ContratoRequest.cs` (campos `EhRecorrente`, `IdCategoria`, `IdConta`, `Dia`, `DiaUtil`, `DataFim`), `Core/Application/Validations/ContratoRequestValidator.cs:14`, `Core/Application/Services/ContratoAppService.cs:58` (`AdicionarAsync`), `Core/Domain/Services/LancamentoHelper.cs` (meses/vencimento) |
| Erros | `CATEGORIA_OBRIGATORIA`, `CONTA_OBRIGATORIA`, `DIA_INVALIDO`, `DIA_UTIL_INVALIDO`, `DATA_FIM_OBRIGATORIA`, `PERIODO_INVALIDO` (400); `NENHUMA_RECEITA_GERADA` (422) |
| Cenários | `C2.1` recorrente sem categoria → 400; `C2.2` recorrente sem conta → 400; `C2.3` dia 0 ou 32 → 400; `C2.4` dia útil com dia 6 → 400; `C2.5` data fim anterior ao início → 400; `C2.6` válido gera regra + parcelas (cobertura parcial: geração exige banco, validada na suite xUnit) |

## R3 — Receita: um vínculo por vez

Receita aceita no máximo um vínculo: `IdParceria` **ou** `IdContrato`,
nunca os dois. Vínculo aponta para registro ativo do próprio usuário.

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Application/Validations/ReceitaRequestValidator.cs:16`, `Core/Application/Services/ReceitaAppService.cs:104` (`AdicionarAsync`), `:184` (`AtualizarAsync`) |
| Erros | `RECEITA_VINCULO_DUPLO` (400); `PARCERIA_INVALIDA`, `CONTRATO_INVALIDO` (400); `RECEITA_ACESSO_NEGADO` (403) |
| Cenários | `C3.1` parceria + contrato juntos → 400 `RECEITA_VINCULO_DUPLO`; `C3.2` só parceria → válido; `C3.3` só contrato → válido; `C3.4` sem vínculo → válido |

## R4 — Parceria: encerrar só sem pendências

Parceria tem valor cheio + % do parceiro. Encerrar exige falta receber
(sobre o valor cheio) **e** falta pagar (sobre a parte do parceiro) zeradas.

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Application/Services/ParceriaAppService.cs:78` (`EncerrarAsync`), `Core/Domain/Entities/Parceria.cs:20` (`Criar`) |
| Erros | `PARCERIA_COM_PENDENCIAS` (422); `PERCENTUAL_INVALIDO` (400); `PARCERIA_ACESSO_NEGADO` (403) |
| Cenários | `C4.1` percentual 150 → 400 `PERCENTUAL_INVALIDO`; `C4.2` encerrar com pendência → 422 (lógica de serviço, cobertura parcial no diagnóstico) |

## R5 — Categorias compartilhadas: dono ou admin

Leitura para todos; editar/excluir só o dono (`IdUsuario`) ou admin,
senão 403. Toda mutação (incluindo subcategoria) grava `CategoriaHistorico`.
Excluir com lançamentos vinculados é bloqueado.

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Application/Services/CategoriaBaseAppService.cs` (posse + auditoria), `Core/Application/Services/CategoriaReceitaAppService.cs:24` (vínculos) |
| Erros | `Erro.Permissao` (403); `CATEGORIA_COM_VINCULOS` (422, verificar código vigente) |
| Cenários | `C5.1` criar sem nome → 400 `NOME_OBRIGATORIO`; `C5.2` posse/auditoria (lógica de serviço, cobertura parcial no diagnóstico) |

## R6 — Dashboard previsto (informativa)

Do mês corrente em diante, valores incorporam previsão (regras vigentes
descontando o materializado por `IdRegra`); meses passados mostram só o
realizado. Detalhe em `doc/front.md` (Indicadores do dashboard mensal).

| Aspecto | Detalhe |
|---|---|
| Vive em | `Core/Application/Services/DashboardAppService.cs`, `Core/Domain/Services/LancamentoHelper.cs`, `doc/front.md:126` |
| Cenários | `C6.1` previsão jan–mar/2026 gera 3 meses; `C6.2` dia 31 em fev/2026 vence dia 28 (matemática da previsão; agregação do dashboard com cobertura parcial) |

## Acesso à tela /testes (módulo `qa`)

A tela `/testes` e o `GET /api/diagnostico` exigem admin **com toggle
liberado** — sem bypass. Ausência da linha `qa` em `PermissaoUsuario`
equivale a negado (padrão desligado para todos).

Pontos de registro do módulo (todos obrigatórios ao mexer neste fluxo):

| Ponto | Arquivo |
|---|---|
| Constante `MODULO_QA = 'qa'` | `Web/.../core/models/permissao.model.ts` |
| Toggle em Permissões especiais (leitura/gravação, inclusive para admin) | `Web/.../features/usuarios/usuarios.component.ts/html` |
| Leitura sem bypass de admin | `Web/.../core/services/auth.service.ts` (`temQA`) |
| Bloqueio da rota manual | `Web/.../core/guards/qa.guard.ts` + `app.routes.ts` |
| Atalho na home (só admin + `temQA`) | `Web/.../features/home/home.component.ts` (seção ACESSO) |
| Bloqueio da API (403 `QA_ACESSO_NEGADO`) | `API/.../Controllers/v1/DiagnosticoController.cs` |

Entidade `PermissaoUsuario` aceita qualquer string em `Modulo`
(`NVARCHAR(50)`); nenhum seed cria a linha `qa` — ela nasce no primeiro
salvamento via drawer de usuários.

## Débitos técnicos conhecidos

- `TransactionScope` aninhado: `ContratoAppService.AdicionarAsync` abre
  `TransactionScope` e `ReceitaRepository.InserirEmMassaAsync` abre transação
  própria (`BeginTransaction`). Funciona, mas acoplamento transacional frágil.
- Cobertura parcial no diagnóstico para lógicas de serviço que exigem banco
  (encerrar contrato/parceria com pendência, posse/auditoria de categoria,
  agregação do dashboard) — cobrir via xUnit com mocks.
- `npm run lint`: 117 warnings (débitos documentados, 0 erros).
