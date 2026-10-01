#Requires -Version 5.1
<#
.SYNOPSIS
    Sincroniza o banco de dados local (SQL Server) com os bancos Neon (neondb e neondb_bravo).

.DESCRIPTION
    - neondb: atualiza somente a estrutura (DDL)
    - neondb_bravo: atualiza estrutura (DDL) e dados (DML)

.NOTES
    Configuracao: .env (ignorado pelo git) — ver .env.example para o modelo sem segredos.
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
    if (-not $envVars.ContainsKey($Key) -or [string]::IsNullOrWhiteSpace($envVars[$Key])) {
        Write-Error "Variavel ausente no .env: $Key"
        exit 1
    }
    return $envVars[$Key]
}

$sqlServerName = Get-RequiredEnv 'SQLSERVER_SERVER'
$sqlDatabaseName = Get-RequiredEnv 'SQLSERVER_DATABASE'
$tempDir = Get-RequiredEnv 'SYNC_TEMPDIR'
$psqlPath = Get-RequiredEnv 'PSQL_PATH'
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
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

function Convert-SqlServerTypeToPostgres {
    param([string]$SqlType, $MaxLength = -1, $Precision = -1, $Scale = -1)

    if ($null -eq $MaxLength -or $MaxLength -eq [DBNull]::Value) { $MaxLength = -1 }
    if ($null -eq $Precision -or $Precision -eq [DBNull]::Value) { $Precision = -1 }
    if ($null -eq $Scale -or $Scale -eq [DBNull]::Value) { $Scale = -1 }

    $MaxLength = [int]$MaxLength
    $Precision = [int]$Precision
    $Scale = [int]$Scale

    $type = $SqlType.ToUpper()

    switch -Regex ($type) {
        '^NVARCHAR|^VARCHAR' {
            if ($MaxLength -eq -1) { return 'TEXT' }
            if ($MaxLength -gt 10485760) { return 'TEXT' }
            return "VARCHAR($MaxLength)"
        }
        '^NCHAR|^CHAR' {
            if ($MaxLength -eq -1) { return 'CHAR(1)' }
            return "CHAR($MaxLength)"
        }
        '^TEXT' { return 'TEXT' }
        '^NTEXT' { return 'TEXT' }
        '^INT$' { return 'INTEGER' }
        '^BIGINT' { return 'BIGINT' }
        '^SMALLINT' { return 'SMALLINT' }
        '^TINYINT' { return 'SMALLINT' }
        '^BIT' { return 'BOOLEAN' }
        '^DECIMAL|^NUMERIC' {
            if ($Precision -ge 0 -and $Scale -ge 0) { return "NUMERIC($Precision,$Scale)" }
            return 'NUMERIC'
        }
        '^FLOAT|^REAL' { return 'DOUBLE PRECISION' }
        '^MONEY|^SMALLMONEY' { return 'NUMERIC(19,4)' }
        '^DATETIME|^DATETIME2|^SMALLDATETIME' { return 'TIMESTAMP' }
        '^DATE' { return 'DATE' }
        '^TIME' { return 'TIME' }
        '^UNIQUEIDENTIFIER' { return 'UUID' }
        '^VARBINARY|^BINARY|^IMAGE' { return 'BYTEA' }
        '^XML' { return 'XML' }
        '^JSON' { return 'JSONB' }
        default { return $type }
    }
}

function Get-SqlServerTables {
    param([string]$Server, [string]$Database)

    $query = @"
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA', 'db_owner', 'db_accessadmin', 'db_securityadmin', 'db_ddladmin', 'db_backupoperator', 'db_datareader', 'db_datawriter', 'db_denydatareader', 'db_denydatawriter')
ORDER BY TABLE_SCHEMA, TABLE_NAME
"@

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $tables = @()
    while ($reader.Read()) {
        $tables += [PSCustomObject]@{
            Schema = $reader['TABLE_SCHEMA']
            Name   = $reader['TABLE_NAME']
        }
    }

    $reader.Close()
    $connection.Close()
    return $tables
}

function Get-SqlServerColumns {
    param([string]$Server, [string]$Database, [string]$Schema, [string]$Table)

    $query = @"
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = '$Schema' AND TABLE_NAME = '$Table'
ORDER BY ORDINAL_POSITION
"@

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $columns = @()
    while ($reader.Read()) {
        $maxLen = $reader['CHARACTER_MAXIMUM_LENGTH']
        if ($null -eq $maxLen -or $maxLen -eq [DBNull]::Value) { $maxLen = $null }

        $precision = $reader['NUMERIC_PRECISION']
        if ($null -eq $precision -or $precision -eq [DBNull]::Value) { $precision = $null }

        $scale = $reader['NUMERIC_SCALE']
        if ($null -eq $scale -or $scale -eq [DBNull]::Value) { $scale = $null }

        $default = $reader['COLUMN_DEFAULT']
        if ($null -eq $default -or $default -eq [DBNull]::Value) { $default = $null }

        $columns += [PSCustomObject]@{
            Name           = $reader['COLUMN_NAME']
            SqlType        = $reader['DATA_TYPE']
            MaxLength      = $maxLen
            Precision      = $precision
            Scale          = $scale
            IsNullable     = $reader['IS_NULLABLE']
            Default        = $default
            OrdinalPosition = $reader['ORDINAL_POSITION']
        }
    }

    $reader.Close()
    $connection.Close()
    return $columns
}

function Get-SqlServerPrimaryKeys {
    param([string]$Server, [string]$Database, [string]$Schema, [string]$Table)

    $query = @"
SELECT kcu.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
    AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
    AND tc.TABLE_NAME = kcu.TABLE_NAME
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
    AND tc.TABLE_SCHEMA = '$Schema'
    AND tc.TABLE_NAME = '$Table'
ORDER BY kcu.ORDINAL_POSITION
"@

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $columns = @()
    while ($reader.Read()) {
        $columns += $reader['COLUMN_NAME']
    }

    $reader.Close()
    $connection.Close()
    return $columns
}

function Get-SqlServerForeignKeys {
    param([string]$Server, [string]$Database, [string]$Schema, [string]$Table)

    $query = @"
SELECT
    fk.CONSTRAINT_NAME,
    fk.COLUMN_NAME,
    pk.TABLE_SCHEMA AS REF_SCHEMA,
    pk.TABLE_NAME AS REF_TABLE,
    pk.COLUMN_NAME AS REF_COLUMN
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk
    ON rc.CONSTRAINT_NAME = fk.CONSTRAINT_NAME
    AND rc.CONSTRAINT_SCHEMA = fk.CONSTRAINT_SCHEMA
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk
    ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
    AND rc.UNIQUE_CONSTRAINT_SCHEMA = pk.CONSTRAINT_SCHEMA
    AND fk.ORDINAL_POSITION = pk.ORDINAL_POSITION
WHERE fk.TABLE_SCHEMA = '$Schema'
    AND fk.TABLE_NAME = '$Table'
"@

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $fks = @()
    while ($reader.Read()) {
        $fks += [PSCustomObject]@{
            ConstraintName = $reader['CONSTRAINT_NAME']
            ColumnName     = $reader['COLUMN_NAME']
            RefSchema      = $reader['REF_SCHEMA']
            RefTable       = $reader['REF_TABLE']
            RefColumn      = $reader['REF_COLUMN']
        }
    }

    $reader.Close()
    $connection.Close()
    return $fks
}

function Get-SqlServerIndexes {
    param([string]$Server, [string]$Database, [string]$Schema, [string]$Table)

    $query = @"
SELECT
    i.name AS INDEX_NAME,
    i.is_unique,
    i.is_primary_key,
    COL_NAME(ic.object_id, ic.column_id) AS COLUMN_NAME,
    ic.key_ordinal
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('$Schema.$Table')
    AND i.type > 0
    AND i.is_primary_key = 0
ORDER BY i.name, ic.key_ordinal
"@

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $indexes = @{}
    while ($reader.Read()) {
        $indexName = $reader['INDEX_NAME']
        if (-not $indexes.ContainsKey($indexName)) {
            $indexes[$indexName] = [PSCustomObject]@{
                Name      = $indexName
                IsUnique  = $reader['is_unique']
                Columns   = @()
            }
        }
        $indexes[$indexName].Columns += $reader['COLUMN_NAME']
    }

    $reader.Close()
    $connection.Close()
    return $indexes.Values
}

function New-PostgresCreateTable {
    param(
        [string]$Schema,
        [string]$Table,
        [array]$Columns,
        [array]$PrimaryKeys
    )

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $lines = @()
    $lines += "CREATE TABLE IF NOT EXISTS $fullName ("

    $columnDefs = @()
    foreach ($col in $Columns) {
        $pgType = Convert-SqlServerTypeToPostgres -SqlType $col.SqlType -MaxLength $col.MaxLength -Precision $col.Precision -Scale $col.Scale
        $nullable = if ($col.IsNullable -eq 'YES') { '' } else { ' NOT NULL' }
        $default = ''
        if ($col.Default -and $col.Default -ne '()') {
            $defaultVal = $col.Default -replace '^\(|\)$', ''
            if ($pgType -eq 'BOOLEAN') {
                if ($defaultVal -match '0') { $defaultVal = 'false' }
                elseif ($defaultVal -match '1') { $defaultVal = 'true' }
            }
            $default = " DEFAULT $defaultVal"
        }
        $columnDefs += "    $($col.Name) $pgType$nullable$default"
    }

    if ($PrimaryKeys.Count -gt 0) {
        $pkCols = $PrimaryKeys -join ', '
        $columnDefs += "    CONSTRAINT pk_$Table PRIMARY KEY ($pkCols)"
    }

    $lines += $columnDefs -join ",`n"
    $lines += ");"
    return $lines -join "`n"
}

function New-PostgresForeignKeys {
    param(
        [string]$Schema,
        [string]$Table,
        [array]$ForeignKeys
    )

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $lines = @()

    foreach ($fk in $ForeignKeys) {
        $refFullName = if ($fk.RefSchema -eq 'dbo') { $fk.RefTable } else { "$($fk.RefSchema).$($fk.RefTable)" }
        $lines += "DO `$$`$"
        $lines += "BEGIN"
        $lines += "    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = '$($fk.ConstraintName)') THEN"
        $lines += "        ALTER TABLE $fullName ADD CONSTRAINT $($fk.ConstraintName) FOREIGN KEY ($($fk.ColumnName)) REFERENCES $refFullName($($fk.RefColumn));"
        $lines += "    END IF;"
        $lines += "END `$$`;"
    }

    return $lines -join "`n"
}

function New-PostgresIndexes {
    param(
        [string]$Schema,
        [string]$Table,
        [array]$Indexes
    )

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $lines = @()

    foreach ($idx in $Indexes) {
        $unique = if ($idx.IsUnique) { 'UNIQUE ' } else { '' }
        $cols = $idx.Columns -join ', '
        $lines += "CREATE ${unique}INDEX IF NOT EXISTS ""$($idx.Name)"" ON $fullName ($cols);"
    }

    return $lines -join "`n"
}

function Get-SqlServerTableData {
    param([string]$Server, [string]$Database, [string]$Schema, [string]$Table)

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $query = "SELECT * FROM $fullName"

    $connectionString = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $reader = $command.ExecuteReader()

    $data = @()
    while ($reader.Read()) {
        $row = @{}
        for ($i = 0; $i -lt $reader.FieldCount; $i++) {
            $colName = $reader.GetName($i)
            $value = $reader.GetValue($i)
            $row[$colName] = $value
        }
        $data += [PSCustomObject]$row
    }

    $reader.Close()
    $connection.Close()
    return $data
}

function Convert-ToPostgresValue {
    param($Value, [string]$SqlType)

    if ($null -eq $Value -or $Value -eq [DBNull]::Value) { return 'NULL' }

    $type = $SqlType.ToUpper()

    switch -Regex ($type) {
        '^BIT' {
            if ($Value -eq $true -or $Value -eq 1) { return 'TRUE' }
            return 'FALSE'
        }
        '^DATETIME|^DATETIME2|^SMALLDATETIME|^DATE|^TIME' {
            $dt = [DateTime]$Value
            return "'$($dt.ToString('yyyy-MM-dd HH:mm:ss'))'"
        }
        '^UNIQUEIDENTIFIER' {
            return "'$Value'"
        }
        '^VARBINARY|^BINARY|^IMAGE' {
            $bytes = [byte[]]$Value
            $hex = ($bytes | ForEach-Object { $_.ToString('x2') }) -join ''
            return "decode('$hex', 'hex')"
        }
        '^INT|^BIGINT|^SMALLINT|^TINYINT|^DECIMAL|^NUMERIC|^FLOAT|^REAL|^MONEY|^SMALLMONEY' {
            return "$Value"
        }
        default {
            $str = "$Value"
            $str = $str -replace "''", "''"
            $str = $str -replace "'", "''"
            return "'$str'"
        }
    }
}

function New-PostgresInsertStatements {
    param(
        [string]$Schema,
        [string]$Table,
        [array]$Columns,
        [array]$Data
    )

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $colNames = ($Columns | ForEach-Object { $_.Name }) -join ', '
    $lines = @()

    foreach ($row in $Data) {
        $values = @()
        foreach ($col in $Columns) {
            $val = Convert-ToPostgresValue -Value $row.($col.Name) -SqlType $col.SqlType
            $values += $val
        }
        $valuesStr = $values -join ', '
        $lines += "INSERT INTO $fullName ($colNames) VALUES ($valuesStr);"
    }

    return $lines -join "`n"
}

function Invoke-Psql {
    param(
        [string]$DbHost,
        [int]$Port,
        [string]$Database,
        [string]$Username,
        [string]$Password,
        [string]$SqlFile
    )

    $env:PGPASSWORD = $Password

    Write-Log "Executando psql em $Database..." "INFO"

    & $psqlPath -h $DbHost -p $Port -U $Username -d $Database -f $SqlFile -v "ON_ERROR_STOP=1" --no-psqlrc

    if ($LASTEXITCODE -ne 0) {
        Write-Log "Erro ao executar psql em $Database (exit code: $LASTEXITCODE)" "ERROR"
        throw "Falha ao aplicar script no $Database"
    }

    Write-Log "psql executado com sucesso em $Database" "SUCCESS"
}

function Export-DdlScript {
    param(
        [string]$Server,
        [string]$Database,
        [string]$OutputPath
    )

    Write-Log "Extraindo DDL do SQL Server ($Server/$Database)..." "INFO"
    $tables = Get-SqlServerTables -Server $Server -Database $Database
    Write-Log "Tabelas encontradas: $($tables.Count)" "INFO"

    $script = @()
    $script += "-- DDL gerado automaticamente em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $script += "-- Origem: SQL Server $Server/$Database"
    $script += ""

    foreach ($table in $tables) {
        Write-Log "  Processando tabela: $($table.Schema).$($table.Name)" "INFO"

        $columns = Get-SqlServerColumns -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name
        $pks = Get-SqlServerPrimaryKeys -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name
        $fks = Get-SqlServerForeignKeys -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name
        $indexes = Get-SqlServerIndexes -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name

        $script += New-PostgresCreateTable -Schema $table.Schema -Table $table.Name -Columns $columns -PrimaryKeys $pks
        $script += ""

        if ($indexes.Count -gt 0) {
            $script += New-PostgresIndexes -Schema $table.Schema -Table $table.Name -Indexes $indexes
            $script += ""
        }

        if ($fks.Count -gt 0) {
            $script += New-PostgresForeignKeys -Schema $table.Schema -Table $table.Name -ForeignKeys $fks
            $script += ""
        }
    }

    $script | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Log "DDL exportado para: $OutputPath" "SUCCESS"
}

function Export-DmlScript {
    param(
        [string]$Server,
        [string]$Database,
        [string]$OutputPath
    )

    Write-Log "Extraindo dados do SQL Server ($Server/$Database)..." "INFO"
    $tables = Get-SqlServerTables -Server $Server -Database $Database
    Write-Log "Tabelas encontradas: $($tables.Count)" "INFO"

    $script = @()
    $script += "-- DML gerado automaticamente em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $script += "-- Origem: SQL Server $Server/$Database"
    $script += ""

    foreach ($table in $tables) {
        Write-Log "  Exportando dados: $($table.Schema).$($table.Name)" "INFO"

        $columns = Get-SqlServerColumns -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name
        $data = Get-SqlServerTableData -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name

        if ($data.Count -eq 0) {
            Write-Log "    Tabela vazia, pulando..." "WARN"
            continue
        }

        Write-Log "    $($data.Count) registros exportados" "INFO"
        $script += New-PostgresInsertStatements -Schema $table.Schema -Table $table.Name -Columns $columns -Data $data
        $script += ""
    }

    $script | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Log "DML exportado para: $OutputPath" "SUCCESS"
}

function Clear-NeonDatabase {
    param(
        [string]$DbHost,
        [int]$Port,
        [string]$Database,
        [string]$Username,
        [string]$Password
    )

    Write-Log "Limpando banco $Database (removendo todas as tabelas)..." "WARN"

    $dropScript = @"
DO `$$`$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END `$$`$;
"@

    $dropFile = Join-Path $tempDir "drop_all.sql"
    $dropScript | Out-File -FilePath $dropFile -Encoding UTF8

    Invoke-Psql -DbHost $DbHost -Port $Port -Database $Database -Username $Username -Password $Password -SqlFile $dropFile
    Write-Log "Banco $Database limpo com sucesso" "SUCCESS"
}

function Main {
    Write-Log "========================================" "INFO"
    Write-Log "Sincronizacao de Banco de Dados" "INFO"
    Write-Log "========================================" "INFO"
    Write-Log "Modo: $(if ($DryRun) { 'DRY RUN (sem aplicar)' } else { 'APLICAR' })" "INFO"
    Write-Log ""

    $sqlServer = $sqlServerName
    $sqlDatabase = $sqlDatabaseName

    $ddlFile = Join-Path $tempDir "ddl_neon.sql"
    $dmlFile = Join-Path $tempDir "dml_neon.sql"

    Export-DdlScript -Server $sqlServer -Database $sqlDatabase -OutputPath $ddlFile

    if (-not $SkipBravo) {
        Export-DmlScript -Server $sqlServer -Database $sqlDatabase -OutputPath $dmlFile
    }

    if ($DryRun) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "DRY RUN - Scripts gerados em:" "INFO"
        Write-Log "  DDL: $ddlFile" "INFO"
        if (-not $SkipBravo) {
            Write-Log "  DML: $dmlFile" "INFO"
        }
        Write-Log "========================================" "INFO"
        return
    }

    if (-not $SkipNeondb) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "Sincronizando neondb (somente estrutura)..." "INFO"
        Write-Log "========================================" "INFO"

        Invoke-Psql -DbHost (Get-RequiredEnv 'NEON_HOST') -Port ([int](Get-RequiredEnv 'NEON_PORT')) -Database (Get-RequiredEnv 'NEON_DB') -Username (Get-RequiredEnv 'NEON_USER') -Password (Get-RequiredEnv 'NEON_PASSWORD') -SqlFile $ddlFile
        Write-Log "neondb sincronizado com sucesso!" "SUCCESS"
    }

    if (-not $SkipBravo) {
        Write-Log ""
        Write-Log "========================================" "INFO"
        Write-Log "Sincronizando neondb_bravo (estrutura + dados)..." "INFO"
        Write-Log "========================================" "INFO"

        $bravoHost = Get-RequiredEnv 'BRAVO_HOST'
        $bravoPort = [int](Get-RequiredEnv 'BRAVO_PORT')
        $bravoDb = Get-RequiredEnv 'BRAVO_DB'
        $bravoUser = Get-RequiredEnv 'BRAVO_USER'
        $bravoPassword = Get-RequiredEnv 'BRAVO_PASSWORD'
        Clear-NeonDatabase -DbHost $bravoHost -Port $bravoPort -Database $bravoDb -Username $bravoUser -Password $bravoPassword
        Invoke-Psql -DbHost $bravoHost -Port $bravoPort -Database $bravoDb -Username $bravoUser -Password $bravoPassword -SqlFile $ddlFile
        Invoke-Psql -DbHost $bravoHost -Port $bravoPort -Database $bravoDb -Username $bravoUser -Password $bravoPassword -SqlFile $dmlFile
        Write-Log "neondb_bravo sincronizado com sucesso!" "SUCCESS"
    }

    Write-Log ""
    Write-Log "========================================" "INFO"
    Write-Log "Sincronizacao concluida!" "SUCCESS"
    Write-Log "========================================" "INFO"
}

Main

Write-Host ""
Write-Host "Pressione Enter para fechar..." -ForegroundColor Cyan
Read-Host
