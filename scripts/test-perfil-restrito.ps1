# Regressão de permissões com perfil restrito (usuário teste).
# Uso: .\scripts\test-perfil-restrito.ps1 [-ApiBase http://localhost:5178]
# Pré-requisito: API no ar (dotnet run --project src/PortalFinanceiro.API).
# Não cria dados: os POSTs usam corpo vazio e o filtro de escrita (403) roda
# antes do model binding, então nada é persistido. Ao final, restaura os
# níveis originais do usuário teste.
param(
  [string]$ApiBase = "http://localhost:5178",
  [string]$AdminEmail = "admin@portal.com",
  [string]$AdminSenha = "senhasenha",
  [string]$TesteEmail = "teste@portal.com",
  [string]$TesteSenha = "123456"
)

$ErrorActionPreference = "Stop"
$falhas = 0

function Assert($condicao, $rotulo) {
  if ($condicao) { Write-Output "PASS $rotulo" }
  else { Write-Output "FAIL $rotulo"; $script:falhas++ }
}

function SemEnvelope($r) {
  if ($r -isnot [array] -and $r.dados) { return $r.dados } else { return $r }
}

function Login($email, $senha) {
  $body = "{`"email`":`"$email`",`"senha`":`"$senha`"}"
  $r = Invoke-RestMethod -Uri "$ApiBase/api/auth/login" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 15
  return SemEnvelope $r
}

function Nivel($loginResp, $modulo) {
  $p = $loginResp.permissoes | Where-Object { $_.modulo -eq $modulo }
  if ($p) { return [int]$p.nivel } else { return -1 }
}

function StatusHttp($metodo, $url, $token, $body) {
  try {
    if ($body) {
      Invoke-RestMethod -Uri $url -Method $metodo -Headers @{ Authorization = "Bearer $token" } -Body $body -ContentType "application/json" -TimeoutSec 15 | Out-Null
    } else {
      Invoke-RestMethod -Uri $url -Method $metodo -Headers @{ Authorization = "Bearer $token" } -TimeoutSec 15 | Out-Null
    }
    return 200
  } catch {
    return [int]$_.Exception.Response.StatusCode
  }
}

$admin = Login $AdminEmail $AdminSenha
$adminToken = $admin.token
$usuarios = SemEnvelope (Invoke-RestMethod -Uri "$ApiBase/api/usuarios" -Headers @{ Authorization = "Bearer $adminToken" } -TimeoutSec 15)
$testeId = ($usuarios | Where-Object { $_.email -eq $TesteEmail }).id
Assert ($testeId -ne $null) "localiza usuario teste"

$basePerms = SemEnvelope (Invoke-RestMethod -Uri "$ApiBase/api/usuarios/$testeId/permissoes" -Headers @{ Authorization = "Bearer $adminToken" } -TimeoutSec 15)

try {
  $alvos = @(
    @{ modulo = "receitas"; lista = "/api/receitas?mes=9&ano=2026"; escreve = "/api/receitas" },
    @{ modulo = "despesas"; lista = "/api/despesas?mes=9&ano=2026"; escreve = "/api/despesas" },
    @{ modulo = "parcerias"; lista = "/api/parcerias"; escreve = "/api/parcerias" },
    @{ modulo = "contratos"; lista = "/api/contratos"; escreve = "/api/contratos" }
  )
  foreach ($a in $alvos) {
    foreach ($nivel in @(0, 1, 2)) {
      $putBody = "[{`"modulo`":`"$($a.modulo)`",`"nivel`":$nivel}]"
      Invoke-RestMethod -Uri "$ApiBase/api/usuarios/$testeId/permissoes" -Method Put -Headers @{ Authorization = "Bearer $adminToken" } -Body $putBody -ContentType "application/json" -TimeoutSec 15 | Out-Null
      $t = Login $TesteEmail $TesteSenha
      Assert ((Nivel $t $($a.modulo)) -eq $nivel) "$($a.modulo)=$nivel reflete no login"
      Assert ((StatusHttp "Get" ($ApiBase + $a.lista) $t.token $null) -eq 200) "$($a.modulo)=$nivel leitura lista (so exige auth)"
      $st = StatusHttp "Post" ($ApiBase + $a.escreve) $t.token "{}"
      if ($nivel -eq 2) { Assert ($st -ne 403) "$($a.modulo)=$nivel escrita nao bloqueada (status $st)" }
      else { Assert ($st -eq 403) "$($a.modulo)=$nivel escrita bloqueada 403 (status $st)" }
    }
  }
} finally {
  $restore = ($basePerms | ForEach-Object { "{`"modulo`":`"$($_.modulo)`",`"nivel`":$($_.nivel)}" }) -join ","
  Invoke-RestMethod -Uri "$ApiBase/api/usuarios/$testeId/permissoes" -Method Put -Headers @{ Authorization = "Bearer $adminToken" } -Body "[$restore]" -ContentType "application/json" -TimeoutSec 15 | Out-Null
  Write-Output "Permissoes originais restauradas."
}

Write-Output ""
Write-Output "Checklist manual (com o teste logado de novo a cada troca de nivel):"
Write-Output "  nivel 0: menu oculto; URL direta redireciona para /; sem botao incluir"
Write-Output "  nivel 1: menu visivel e lista carrega; sem botao incluir/editar/excluir"
Write-Output "  nivel 2: botao incluir visivel (receitas/despesas, parcerias, contratos, contas)"
Write-Output "  REGRA DE OURO: sair/entrar do usuario teste apos mudar permissao (permissoes congeladas no login)"

if ($falhas -gt 0) { Write-Output "RESULTADO: $falhas falha(s)"; exit 1 }
Write-Output "RESULTADO: tudo PASS"
