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

    if ($null -eq $MaxLength -or $MaxLength -is [DBNull]) { $MaxLength = -1 }
    if ($null -eq $Precision -or $Precision -is [DBNull]) { $Precision = -1 }
    if ($null -eq $Scale -or $Scale -is [DBNull]) { $Scale = -1 }

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
        if ($null -eq $maxLen -or $maxLen -is [DBNull]) { $maxLen = $null }

        $precision = $reader['NUMERIC_PRECISION']
        if ($null -eq $precision -or $precision -is [DBNull]) { $precision = $null }

        $scale = $reader['NUMERIC_SCALE']
        if ($null -eq $scale -or $scale -is [DBNull]) { $scale = $null }

        $default = $reader['COLUMN_DEFAULT']
        if ($null -eq $default -or $default -is [DBNull]) { $default = $null }

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
            $defaultVal = $col.Default.Trim()
            # ((1)) -> (1) -> 1: o SQL Server empilha parenteses nos defaults
            while ($defaultVal -match '^\((.*)\)$') {
                $defaultVal = $Matches[1].Trim()
            }
            if ($pgType -eq 'BOOLEAN') {
                if ($defaultVal -eq '1') { $defaultVal = 'true' }
                elseif ($defaultVal -eq '0') { $defaultVal = 'false' }
            }
            elseif ($defaultVal -match '(?i)getdate|getutcdate|sysdatetime|newid') {
                # funcoes do SQL Server sem equivalente direto: DEFAULT no Postgres vira
                # CURRENT_TIMESTAMP e o UUID passa a ser gerado pela aplicacao
                if ($pgType -match 'TIMESTAMP') { $defaultVal = 'CURRENT_TIMESTAMP' }
                else { $defaultVal = '' }
            }
            if ($defaultVal) {
                $default = " DEFAULT $defaultVal"
            }
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

    # Dollar-quoting do Postgres: sem ele o psql interpretaria o corpo do bloco.
    # Montado com [char]36 para evitar escape dentro da propria string.
    $dq = [string][char]36

    foreach ($fk in $ForeignKeys) {
        $refFullName = if ($fk.RefSchema -eq 'dbo') { $fk.RefTable } else { "$($fk.RefSchema).$($fk.RefTable)" }
        $lines += "DO $dq$dq"
        $lines += 'BEGIN'
        $lines += "    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE LOWER(conname) = LOWER('$($fk.ConstraintName)')) THEN"
        $lines += "        ALTER TABLE $fullName ADD CONSTRAINT $($fk.ConstraintName) FOREIGN KEY ($($fk.ColumnName)) REFERENCES $refFullName($($fk.RefColumn));"
        $lines += '    END IF;'
        $lines += "END $dq$dq;"
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

    if ($null -eq $Value -or $Value -is [DBNull]) { return 'NULL' }

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
            $str = "$Value".Replace("'", "''")
            return "'$str'"
        }
    }
}

function Sort-RowsBySelfReference {
    <#
    .SYNOPSIS
        Ordena os INSERTs de uma tabela que referencia a si mesma (ex.: categoria pai/filha).
    .DESCRIPTION
        O Postgres valida a FK no proprio INSERT, entao o pai precisa entrar antes do filho.
        Em caso de ciclo (impossivel na hierarquia de categorias, mas por seguranca) o
        restante e emitido como veio, para o gerador nunca travar.
    #>
    param(
        [array]$Rows,
        [array]$SelfForeignKeys,
        [string]$KeyColumn
    )

    if ($SelfForeignKeys.Count -eq 0 -or $Rows.Count -eq 0) { return $Rows }

    $known = @{}
    foreach ($row in $Rows) { $known["$($row.($KeyColumn))"] = $true }

    $pending = [System.Collections.Generic.List[object]]::new()
    foreach ($row in $Rows) { $pending.Add($row) }

    $emitted = @{}
    $ordered = [System.Collections.Generic.List[object]]::new()

    while ($pending.Count -gt 0) {
        $progressed = $false

        for ($i = 0; $i -lt $pending.Count; $i++) {
            $row = $pending[$i]
            $ready = $true

            foreach ($fk in $SelfForeignKeys) {
                $reference = $row.($fk.ColumnName)
                if ($null -eq $reference -or $reference -is [DBNull]) { continue }
                $referenceKey = "$reference"
                if ($known.ContainsKey($referenceKey) -and -not $emitted.ContainsKey($referenceKey)) {
                    $ready = $false
                    break
                }
            }

            if (-not $ready) { continue }

            $ordered.Add($row)
            $emitted["$($row.($KeyColumn))"] = $true
            $pending.RemoveAt($i)
            $progressed = $true
            break
        }

        if (-not $progressed) {
            foreach ($row in $pending) { $ordered.Add($row) }
            break
        }
    }

    return $ordered
}

function New-PostgresInsertStatements {
    param(
        [string]$Schema,
        [string]$Table,
        [array]$Columns,
        [array]$Data,
        [array]$SelfForeignKeys = @(),
        [string]$KeyColumn = 'Id'
    )

    $fullName = if ($Schema -eq 'dbo') { $Table } else { "$Schema.$Table" }
    $colNames = ($Columns | ForEach-Object { $_.Name }) -join ', '
    $lines = @()

    $ordered = Sort-RowsBySelfReference -Rows $Data -SelfForeignKeys $SelfForeignKeys -KeyColumn $KeyColumn

    foreach ($row in $ordered) {
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

    $stdoutFile = Join-Path $tempDir "psql_stdout.log"
    $stderrFile = Join-Path $tempDir "psql_stderr.log"

    $process = Start-Process -FilePath $psqlPath -ArgumentList @(
        "-h", $DbHost,
        "-p", $Port,
        "-U", $Username,
        "-d", $Database,
        "-f", $SqlFile,
        "-v", "ON_ERROR_STOP=1",
        "--no-psqlrc"
    ) -NoNewWindow -Wait -PassThru -RedirectStandardOutput $stdoutFile -RedirectStandardError $stderrFile

    $stderr = Get-Content $stderrFile -Raw -ErrorAction SilentlyContinue
    $stdout = Get-Content $stdoutFile -Raw -ErrorAction SilentlyContinue

    if ($process.ExitCode -ne 0) {
        Write-Log "Erro ao executar psql em $Database (exit code: $($process.ExitCode))" "ERROR"
        if ($stderr) { Write-Log $stderr "ERROR" }
        throw "Falha ao aplicar script no $Database"
    }

    Write-Log "psql executado com sucesso em $Database" "SUCCESS"
    if ($stdout) { Write-Log $stdout "INFO" }
}

function Get-SchemaMetadata {
    <#
    .SYNOPSIS
        Le metadados (colunas, PKs, FKs e indices) de todas as tabelas de origem.
    .DESCRIPTION
        Retorna hashtable nome-da-tabela -> metadados, incluindo DependsOn (tabelas
        referenciadas) para permitir a ordenacao topologica exigida pelo Postgres.
        Tabelas do DbUp (journal de migracao) e schemas de sistema sao ignorados.
    #>
    param([string]$Server, [string]$Database)

    $skip = @('schemaversions')

    $tables = Get-SqlServerTables -Server $Server -Database $Database |
        Where-Object { $_.Schema -eq 'dbo' -and ($skip -notcontains $_.Name.ToLower()) }

    $metadata = @{}
    foreach ($table in $tables) {
        $fks = Get-SqlServerForeignKeys -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name
        $dependsOn = @($fks |
            Where-Object { $_.RefSchema -eq 'dbo' } |
            ForEach-Object { $_.RefTable } |
            Where-Object { $_ -ne $table.Name } |
            Select-Object -Unique)

        $metadata[$table.Name] = @{
            Schema          = $table.Schema
            Columns         = (Get-SqlServerColumns -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name)
            PrimaryKeys     = @(Get-SqlServerPrimaryKeys -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name)
            ForeignKeys     = @($fks)
            Indexes         = @(Get-SqlServerIndexes -Server $Server -Database $Database -Schema $table.Schema -Table $table.Name)
            DependsOn       = $dependsOn
            SelfForeignKeys = @($fks | Where-Object { $_.RefSchema -eq 'dbo' -and $_.RefTable -eq $table.Name })
        }
    }

    return $metadata
}

function Get-DependencyOrder {
    <#
    .SYNOPSIS
        Ordena as tabelas para que toda dependencia criada antes de quem a referencia.
    .DESCRIPTION
        Depth-first sobre DependsOn. Empates resolvem em ordem alfabetica, garantindo
        saida estavel. Tabelas com ciclo (auto-referencia) nao travam o gerador.
    #>
    param([hashtable]$Metadata)

    $visited = @{}
    $visiting = @{}
    $ordered = New-Object System.Collections.Generic.List[string]

    function Visit {
        param([string]$Name)

        if ($visited.ContainsKey($Name) -or $visiting.ContainsKey($Name)) { return }
        if (-not $Metadata.ContainsKey($Name)) { return }

        $visiting[$Name] = $true
        foreach ($dependency in ($Metadata[$Name].DependsOn | Sort-Object)) {
            Visit -Name $dependency
        }
        $visiting.Remove($Name)
        $visited[$Name] = $true
        $ordered.Add($Name)
    }

    foreach ($name in ($Metadata.Keys | Sort-Object)) {
        Visit -Name $name
    }

    return $ordered
}

function Export-DdlScript {
    param(
        [string]$Server,
        [string]$Database,
        [string]$OutputPath
    )

    Write-Log "Extraindo DDL do SQL Server ($Server/$Database)..." "INFO"
    $metadata = Get-SchemaMetadata -Server $Server -Database $Database
    Write-Log "Tabelas encontradas: $($metadata.Count)" "INFO"

    # O Postgres exige que a tabela referenciada exista antes do ALTER TABLE ... FOREIGN KEY,
    # entao todo CREATE TABLE sai primeiro e as FKs ficam todas no final.
    $order = Get-DependencyOrder -Metadata $metadata

    $script = @()
    $script += "-- DDL gerado automaticamente em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $script += "-- Origem: SQL Server $Server/$Database"
    $script += "-- Fase 1/2: tabelas e indices (ordem topologica)"
    $script += ""

    foreach ($name in $order) {
        $meta = $metadata[$name]
        $script += New-PostgresCreateTable -Schema $meta.Schema -Table $name -Columns $meta.Columns -PrimaryKeys $meta.PrimaryKeys
        $script += ""

        if ($meta.Indexes.Count -gt 0) {
            $script += New-PostgresIndexes -Schema $meta.Schema -Table $name -Indexes $meta.Indexes
            $script += ""
        }
    }

    $script += "-- Fase 2/2: chaves estrangeiras (todas as tabelas ja existem)"
    $script += ""

    foreach ($name in $order) {
        $meta = $metadata[$name]
        if ($meta.ForeignKeys.Count -gt 0) {
            $script += New-PostgresForeignKeys -Schema $meta.Schema -Table $name -ForeignKeys $meta.ForeignKeys
            $script += ""
        }
    }

    if (Test-Path $OutputPath) { Remove-Item $OutputPath -Force -ErrorAction SilentlyContinue }
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
    $metadata = Get-SchemaMetadata -Server $Server -Database $Database
    Write-Log "Tabelas encontradas: $($metadata.Count)" "INFO"

    # Mesma ordem do DDL: sem isso o INSERT falha em chave estrangeira.
    $order = Get-DependencyOrder -Metadata $metadata

    $script = @()
    $script += "-- DML gerado automaticamente em $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $script += "-- Origem: SQL Server $Server/$Database"
    $script += ""

    foreach ($name in $order) {
        $meta = $metadata[$name]
        Write-Log "  Exportando dados: $name" "INFO"

        $data = Get-SqlServerTableData -Server $Server -Database $Database -Schema $meta.Schema -Table $name

        if ($data.Count -eq 0) {
            Write-Log "    Tabela vazia, pulando..." "WARN"
            continue
        }

        Write-Log "    $($data.Count) registros exportados" "INFO"
        $keyColumn = if ($meta.PrimaryKeys.Count -gt 0) { $meta.PrimaryKeys[0] } else { 'Id' }
        $script += New-PostgresInsertStatements -Schema $meta.Schema -Table $name -Columns $meta.Columns -Data $data -SelfForeignKeys $meta.SelfForeignKeys -KeyColumn $keyColumn
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
    if (Test-Path $dropFile) { Remove-Item $dropFile -Force -ErrorAction SilentlyContinue }
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
