#Requires -Version 5.1
<#
.SYNOPSIS
    Sincroniza o banco PostgreSQL local (dev) com os bancos Neon (neondb e neondb_bravo).

.DESCRIPTION
    Origem: PostgreSQL local (portal_financeiro) - o SQL Server foi descontinuado
    como fonte; o banco SQL Server local permanece apenas como backup (nunca
    tocado por este script).
    - neondb: atualiza somente a estrutura (DDL) via pg_dump --schema-only
    - neondb_bravo: recria estrutura e dados (DDL + DML) via pg_dump completo

    O script opera SOMENTE em PostgreSQL. Qualquer host que pareça SQL Server
    (localdb/mssql) é rejeitado, e o banco de origem nunca é limpo.

.NOTES
    Configuracao: .env (ignorado pelo git) - ver .env.example para o modelo sem segredos.
    Uso: .\sincronizar-banco.ps1 [-DryRun] [-SkipNeondb] [-SkipBravo]
#>

[CmdletBinding()]
param(
    [switch]$DryRun,
    [switch]$SkipNeondb,
    [switch]$SkipBravo
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$EnvPath = Join-Path $ScriptDir '.env'

if (-not (Test-Path $EnvPath)) {
    Write-Error "Arquivo de configuracao nao encontrado: $EnvPath (copie .env.example para .env)"
    exit 1
}

$envVars = @{}
foreach ($line in Get-Content $EnvPath) {
    $trimmed = $line.Trim()
    if (-not $trimmed -or $trimmed.StartsWith('#')) { continue }
    $separator = $trimmed.IndexOf('=')
    if ($separator -lt 1) { continue }
    $key = $trimmed.Substring(0, $separator).Trim()
    $value = $trimmed.Substring($separator + 1).Trim().Trim('"').Trim("'")
    $envVars[$key] = $value
}

function Get-RequiredEnv {
    param([string]$Key)
    if (-not $envVars.ContainsKey($Key)) {
        Write-Error "Variavel ausente no .env: $Key"
        exit 1
    }
    # Senha pode ser vazia (auth por trust no banco local); demais chaves nao.
    if ([string]::IsNullOrWhiteSpace($envVars[$Key]) -and $Key -notmatch 'PASSWORD$') {
        Write-Error "Variavel vazia no .env: $Key"
        exit 1
    }
    return $envVars[$Key]
}

function Write-Log {
    param([string]$Message, [string]$Level = 'INFO')
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $color = switch ($Level) {
        'ERROR' { 'Red' }
        'WARN'  { 'Yellow' }
        'SUCCESS' { 'Green' }
        default { 'White' }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

function Get-PgDumpPath {
    param([string]$PsqlPath)
    $dir = [System.IO.Path]::GetDirectoryName($PsqlPath)
    if (-not [string]::IsNullOrWhiteSpace($dir)) {
        return Join-Path $dir 'pg_dump.exe'
    }
    return 'pg_dump'
}

function Assert-IsPostgresTarget {
    param([string]$HostName, [string]$Database)
    if ($HostName -match '(?i)localdb|mssql|sqlserver') {
        throw "Alvo invalido: host '$HostName' parece SQL Server. Este script opera somente em PostgreSQL."
    }
    if ($Database -match '(?i)\.bak|\.mdf|master') {
        throw "Alvo invalido: banco '$Database' parece um banco de sistema/backup SQL Server."
    }
}

function Invoke-Psql {
    param(
        [string]$HostName,
        [int]$Port,
        [string]$Database,
        [string]$Username,
        [string]$Password,
        [string]$SqlFile
    )

    Assert-IsPostgresTarget -HostName $HostName -Database $Database
    $env:PGPASSWORD = $Password
    $psqlPath = Get-RequiredEnv 'PSQL_PATH'

    $args = @(
        "-h", $HostName,
        "-p", $Port,
        "-U", $Username,
        "-d", $Database,
        "-f", $SqlFile,
        "-v", "ON_ERROR_STOP=1",
        "--no-psqlrc"
    )

    Write-Log "Executando psql em $Database..." "INFO"
    $process = Start-Process -FilePath $psqlPath -ArgumentList $args -NoNewWindow -Wait -PassThru -RedirectStandardOutput "$tempDir\psql_stdout.log" -RedirectStandardError "$tempDir\psql_stderr.log"

    $stdout = Get-Content "$tempDir\psql_stdout.log" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "$tempDir\psql_stderr.log" -Raw -ErrorAction SilentlyContinue

    if ($process.ExitCode -ne 0) {
        Write-Log "Erro ao executar psql (exit code: $($process.ExitCode))" "ERROR"
        if ($stderr) { Write-Log $stderr "ERROR" }
        throw "Falha ao aplicar script no $Database"
    }

    Write-Log "psql executado com sucesso em $Database" "SUCCESS"
    if ($stdout) { Write-Log $stdout "INFO" }
}

function Invoke-PgDump {
    param(
        [string]$HostName,
        [int]$Port,
        [string]$Database,
        [string]$Username,
        [string]$Password,
        [string]$OutputPath,
        [switch]$SchemaOnly
    )

    Assert-IsPostgresTarget -HostName $HostName -Database $Database
    $env:PGPASSWORD = $Password
    $pgDump = Get-PgDumpPath -PsqlPath (Get-RequiredEnv 'PSQL_PATH')

    $args = @(
        "-h", $HostName,
        "-p", $Port,
        "-U", $Username,
        "-d", $Database,
        "--no-owner",
        "--no-privileges",
        "-f", $OutputPath
    )
    if ($SchemaOnly) { $args += "--schema-only" }

    Write-Log "pg_dump de $Database -> $OutputPath" "INFO"
    $process = Start-Process -FilePath $pgDump -ArgumentList $args -NoNewWindow -Wait -PassThru -RedirectStandardOutput "$tempDir\pgdump_stdout.log" -RedirectStandardError "$tempDir\pgdump_stderr.log"

    $stderr = Get-Content "$tempDir\pgdump_stderr.log" -Raw -ErrorAction SilentlyContinue
    if ($process.ExitCode -ne 0) {
        Write-Log "Erro no pg_dump (exit code: $($process.ExitCode))" "ERROR"
        if ($stderr) { Write-Log $stderr "ERROR" }
        throw "Falha no pg_dump de $Database"
    }
    Write-Log "pg_dump concluido: $OutputPath" "SUCCESS"
}

function Clear-Database {
    param(
        [string]$HostName,
        [int]$Port,
        [string]$Database,
        [string]$Username,
        [string]$Password,
        [string]$ExcludeDatabase
    )

    Assert-IsPostgresTarget -HostName $HostName -Database $Database
    if ($Database -eq $ExcludeDatabase) {
        throw "Recusa: nao e permitido limpar o banco de origem ($Database)."
    }

    Write-Log "Limpando banco $Database (removendo tabelas public)..." "WARN"
    $dropScript = @'
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END $$;
'@

    $dropFile = Join-Path $tempDir "drop_all.sql"
    $dropScript | Out-File -FilePath $dropFile -Encoding UTF8
    Invoke-Psql -HostName $HostName -Port $Port -Database $Database -Username $Username -Password $Password -SqlFile $dropFile
    Write-Log "Banco $Database limpo com sucesso" "SUCCESS"
}

function Main {
    Write-Log "========================================" "INFO"
    Write-Log "Sincronizacao PostgreSQL local -> Neon" "INFO"
    Write-Log "========================================" "INFO"
    Write-Log "Modo: $(if ($DryRun) { 'DRY RUN (sem aplicar)' } else { 'APLICAR' })" "INFO"
    Write-Log ""

    $source = @{
        Host     = Get-RequiredEnv 'LOCAL_PGHOST'
        Port     = [int](Get-RequiredEnv 'LOCAL_PGPORT')
        Database = Get-RequiredEnv 'LOCAL_PGDB'
        Username = Get-RequiredEnv 'LOCAL_PGUSER'
        Password = Get-RequiredEnv 'LOCAL_PGPASSWORD'
    }

    $schemaFile = Join-Path $tempDir "ddl_neon.sql"
    $fullFile   = Join-Path $tempDir "full_neon.sql"

    Write-Log "Gerando dump do banco de origem $($source.Database)..." "INFO"
    Invoke-PgDump -HostName $source.Host -Port $source.Port -Database $source.Database -Username $source.Username -Password $source.Password -OutputPath $schemaFile -SchemaOnly

    if (-not $SkipBravo) {
        Invoke-PgDump -HostName $source.Host -Port $source.Port -Database $source.Database -Username $source.Username -Password $source.Password -OutputPath $fullFile
    }

    if ($DryRun) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "DRY RUN - Dumps gerados em:" "INFO"
        Write-Log "  DDL: $schemaFile" "INFO"
        if (-not $SkipBravo) { Write-Log "  Dados: $fullFile" "INFO" }
        Write-Log "========================================" "INFO"
        return
    }

    if (-not $SkipNeondb) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "Sincronizando neondb (somente estrutura)..." "INFO"
        Write-Log "========================================" "INFO"

        $target = @{
            Host     = Get-RequiredEnv 'NEON_HOST'
            Port     = [int](Get-RequiredEnv 'NEON_PORT')
            Database = Get-RequiredEnv 'NEON_DB'
            Username = Get-RequiredEnv 'NEON_USER'
            Password = Get-RequiredEnv 'NEON_PASSWORD'
        }
        Invoke-Psql -HostName $target.Host -Port $target.Port -Database $target.Database -Username $target.Username -Password $target.Password -SqlFile $schemaFile
        Write-Log "neondb sincronizado com sucesso!" "SUCCESS"
    }

    if (-not $SkipBravo) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "Sincronizando neondb_bravo (estrutura + dados)..." "INFO"
        Write-Log "========================================" "INFO"

        $bravo = @{
            Host     = Get-RequiredEnv 'BRAVO_HOST'
            Port     = [int](Get-RequiredEnv 'BRAVO_PORT')
            Database = Get-RequiredEnv 'BRAVO_DB'
            Username = Get-RequiredEnv 'BRAVO_USER'
            Password = Get-RequiredEnv 'BRAVO_PASSWORD'
        }
        Clear-Database -HostName $bravo.Host -Port $bravo.Port -Database $bravo.Database -Username $bravo.Username -Password $bravo.Password -ExcludeDatabase $source.Database
        Invoke-Psql -HostName $bravo.Host -Port $bravo.Port -Database $bravo.Database -Username $bravo.Username -Password $bravo.Password -SqlFile $fullFile
        Write-Log "neondb_bravo sincronizado com sucesso!" "SUCCESS"
    }

    Write-Log ""
    Write-Log "========================================" "INFO"
    Write-Log "Sincronizacao concluida!" "SUCCESS"
    Write-Log "========================================" "INFO"
}

$tempDir = Get-RequiredEnv 'SYNC_TEMPDIR'
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
}

Main

if ([Environment]::UserInteractive -and $Host.Name -match 'ConsoleHost') {
    Write-Host ""
    Write-Host "Pressione Enter para fechar..." -ForegroundColor Cyan
    try { Read-Host } catch { }
}