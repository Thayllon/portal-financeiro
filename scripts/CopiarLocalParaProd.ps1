# CopiarLocalParaProd.ps1 — Exporta o banco LocalDB (SQL Server) para um .sql compatível com Neon Postgres
# Uso:
#   1. Ajuste $localConnectionString se seu LocalDB tiver nome diferente
#   2. Rode: powershell -ExecutionPolicy Bypass -File scripts/CopiarLocalParaProd.ps1
#   3. O arquivo portal-financeiro-prod-restore.sql será gerado na pasta raiz
#   4. Execute no Neon (SQL Editor ou psql):
#        psql "postgresql://USER:PASS@HOST/neondb?sslmode=require" -f portal-financeiro-prod-restore.sql
#
# ATENÇÃO: o script gerado faz TRUNCATE CASCADE em todas as tabelas de prod antes de inserir.
# Faça backup do prod antes (Neon -> Branches -> Backup ou pg_dump).

param(
    [string]$localConnectionString = "Server=(localdb)\mssqllocaldb;Database=PortalFinanceiro;Trusted_Connection=True;TrustServerCertificate=True;",
    [string]$saida = "portal-financeiro-prod-restore.sql"
)

$ErrorActionPreference = "Stop"

function Escape-String($v) {
    if ($null -eq $v -or $v -is [DBNull]) { return "NULL" }
    $s = $v.ToString().Replace("'", "''")
    return "'$s'"
}

function Format-Value($value, $typeName) {
    if ($null -eq $value -or $value -is [DBNull]) { return "NULL" }
    $t = $typeName.ToLower()
    if ($t -like "*uniqueidentifier*") { return "'$value'::uuid" }
    if ($t -like "*nvarchar*" -or $t -like "*varchar*" -or $t -like "*char*" -or $t -like "*text*") { return Escape-String $value }
    if ($t -like "*bit*") {
        if ($value -is [bool]) { return $(if ($value) { "TRUE" } else { "FALSE" }) }
        return $(if ([int]$value -ne 0) { "TRUE" } else { "FALSE" })
    }
    if ($t -like "*datetime*") {
        $dt = [DateTime]$value
        return "'$($dt.ToString("yyyy-MM-dd HH:mm:ss.fff"))'::timestamp"
    }
    if ($t -like "*decimal*" -or $t -like "*numeric*" -or $t -like "*money*") {
        return $value.ToString().Replace(",", ".")
    }
    if ($t -like "*int*" -or $t -like "*bigint*" -or $t -like "*smallint*") { return "$value" }
    return Escape-String $value
}

$tabelas = @(
    "Usuario",
    "ContaBancaria",
    "Pessoa",
    "CategoriaReceita",
    "CategoriaDespesa",
    "CategoriaServico",
    "PermissaoUsuario",
    "Parceria",
    "RegraReceita",
    "RegraDespesa",
    "Receita",
    "Despesa",
    "ReceitaServico",
    "DespesaServico",
    "CategoriaHistorico"
)

$ordemTruncate = @(
    "CategoriaHistorico","ReceitaServico","DespesaServico","Receita","Despesa",
    "RegraReceita","RegraDespesa","Parceria","PermissaoUsuario",
    "CategoriaServico","CategoriaDespesa","CategoriaReceita",
    "Pessoa","ContaBancaria","Usuario"
)

Write-Host "Conectando no LocalDB..." -ForegroundColor Cyan
$conn = New-Object System.Data.SqlClient.SqlConnection($localConnectionString)
$conn.Open()

$saidaPath = Join-Path (Get-Location) $saida
$writer = [System.IO.StreamWriter]::new($saidaPath, $false, [System.Text.Encoding]::UTF8)

$writer.WriteLine("-- Backup gerado em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') a partir do LocalDB")
$writer.WriteLine("-- Restaure no Neon com: psql ""$env:DATABASE_URL"" -f $saida")
$writer.WriteLine("-- ATENÇÃO: faz TRUNCATE CASCADE — apaga todos os dados de prod antes de inserir")
$writer.WriteLine("BEGIN;")
$writer.WriteLine("")

$writer.WriteLine("-- Desativa FKs temporariamente (TRUNCATE CASCADE já resolve, mas garantimos)")
foreach ($t in $ordemTruncate) {
    $writer.WriteLine("TRUNCATE TABLE ""$t"" CASCADE;")
}
$writer.WriteLine("")

foreach ($tabela in $tabelas) {
    Write-Host "Exportando $tabela..." -NoNewline
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT * FROM [$tabela]"
    $cmd.CommandTimeout = 120
    $reader = $cmd.ExecuteReader()

    $schema = $reader.GetSchemaTable()
    $colunas = @()
    $tipos = @{}
    foreach ($row in $schema.Rows) {
        $colunas += $row["ColumnName"]
        $tipos[$row["ColumnName"]] = $row["DataTypeName"]
    }

    $count = 0
    $batch = 0
    while ($reader.Read()) {
        if ($count % 500 -eq 0) {
            if ($batch -gt 0) { $writer.WriteLine(";") }
            $cols = ($colunas | ForEach-Object { """$_""" }) -join ", "
            $writer.Write("INSERT INTO ""$tabela"" ($cols) VALUES ")
            $batch = 0
        } elseif ($batch -gt 0) {
            $writer.Write(", ")
        }
        $vals = @()
        foreach ($c in $colunas) {
            $vals += Format-Value $reader[$c] $tipos[$c]
        }
        $writer.Write("(" + ($vals -join ", ") + ")")
        $count++
        $batch++
    }
    if ($count -gt 0) { $writer.WriteLine(";") }
    $writer.WriteLine("")
    Write-Host " $count linhas" -ForegroundColor Green
    $reader.Close()
}

$writer.WriteLine("COMMIT;")
$writer.Close()
$conn.Close()

Write-Host ""
Write-Host "Arquivo gerado: $saidaPath" -ForegroundColor Green
Write-Host "Execute no prod (Neon):" -ForegroundColor Yellow
Write-Host '  psql "postgresql://USER:PASS@HOST/neondb?sslmode=require" -f portal-financeiro-prod-restore.sql'
Write-Host "Ou cole o conteúdo no SQL Editor do Neon (https://console.neon.tech)" -ForegroundColor Yellow
