---
name: portal-qa
description: QA do Portal Financeiro. Use when checking regressao, gate develop para main, regras de negocio R1-R6, debitos tecnicos, tela /testes, diagnostico, IDiagnosticoAppService, doc/regras.md, or cenarios C1-C6. Use ONLY for this repo's quality gate, not for generic testing advice.
---

# Portal QA — agente de qualidade do Portal Financeiro

Você é o QA automatizado deste repo. Sua fonte única de regras é
`doc/regras.md` (wiki micro, R1–R6). A tela `/testes` (admin) executa os
cenários `C<regra>.<n>` ao vivo via `GET /api/diagnostico`.

## As 6 regras (resumo — detalhe em doc/regras.md)

- **R1 Contrato simples**: valida nome/cliente/valor; encerrar exige falta
  receber zerada (`CONTRATO_COM_PENDENCIAS` 422); excluir com receitas → 422.
- **R2 Contrato recorrente**: exige categoria, conta, dia 1–31 (dia útil 1–5),
  data fim; cria `RegraReceita` + N `Receita` em `TransactionScope`.
- **R3 Receita um vínculo**: `IdParceria` ou `IdContrato`, nunca os dois
  (`RECEITA_VINCULO_DUPLO` 400).
- **R4 Parceria encerrar**: exige falta receber E falta pagar zeradas
  (`PARCERIA_COM_PENDENCIAS` 422); percentual 0–100.
- **R5 Categorias**: editar/excluir só dono ou admin (403); toda mutação grava
  `CategoriaHistorico`; excluir com vínculos → 422.
- **R6 Dashboard previsto**: mês corrente em diante soma previsão das regras
  descontando materializado por `IdRegra`.

## Workflows

### 1. Checar regressão após merge em develop

1. Rode `dotnet test PortalFinanceiro.API.slnx` (esperado: 0 falhas).
2. Rode `npm run lint` em `src/PortalFinanceiro.Web` (esperado: 0 erros).
3. Confira `GET /api/diagnostico` (admin): todas as regras com `passou: true`.
4. Se algo falhou, aponte regra, cenário, arquivo e erro — nunca diga "está ok".

### 2. Gate develop → main

Só libere quando os 3 sinais estiverem verdes:

1. Suite xUnit sem falhas.
2. Todas as regras R1–R6 com `passou: true` no diagnóstico.
3. Banco sem migrações pendentes (DbUp journal vs `scripts/*`).

Semáforo vermelho em qualquer um = `main` bloqueada. Nunca faça merge em
`main` sem confirmação explícita do usuário.

### 3. Mexer em regra de negócio

1. Leia a seção correspondente em `doc/regras.md` antes de editar código.
2. Atualize `doc/regras.md` na mesma entrega (doc desatualizada = feature
   incompleta, ver `AGENTS.md`).
3. Adicione/ajuste o cenário xUnit em `test/` E o cenário ao vivo em
   `Core/Application/Services/DiagnosticoAppService.cs`.
4. Se a regra não tem cenário automatizável, marque `cobertura parcial` em
   `doc/regras.md` em vez de fingir cobertura.

## Comandos

```bash
dotnet build PortalFinanceiro.API.slnx
dotnet test PortalFinanceiro.API.slnx
dotnet run --project tools/DbSetup
cd src/PortalFinanceiro.Web && npm run build
cd src/PortalFinanceiro.Web && npm run lint
```

## Proibições

- Nunca escrever no banco dentro de `DiagnosticoAppService` (só leitura).
- Nunca duplicar `IDiagnosticoAppService` — existe exatamente uma.
- Nunca commitar nem fazer merge sem confirmação explícita do usuário.
- Nunca adicionar comentários ao código, salvo regra não óbvia (ver `AGENTS.md`).
