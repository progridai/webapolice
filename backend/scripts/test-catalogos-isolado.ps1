$ErrorActionPreference = 'Stop'
$backendDir = Split-Path $PSScriptRoot -Parent
$line = Get-Content -LiteralPath (Join-Path $backendDir '.env.local') | Where-Object { $_ -match '^\s*ConnectionStrings__PostgreSql=' } | Select-Object -First 1
if (-not $line) { throw 'ConnectionStrings__PostgreSql não configurada.' }
$connection = New-Object System.Data.Common.DbConnectionStringBuilder
$connection.set_ConnectionString($line.Substring($line.IndexOf('=') + 1).Trim().Trim('"').Trim("'"))
$sourceDatabase = [string]$connection.get_Item('Database')
if ($sourceDatabase -ne 'webapolice_teste') { throw 'Este script exige a base de desenvolvimento webapolice_teste como origem.' }
$env:PGHOST = [string]$connection.get_Item('Host')
$env:PGPORT = if ($connection.ContainsKey('Port')) { [string]$connection.get_Item('Port') } else { '5432' }
$env:PGUSER = [string]$connection.get_Item('Username')
$env:PGPASSWORD = [string]$connection.get_Item('Password')
$env:PGCONNECT_TIMEOUT = '10'
$env:PGDATABASE = $sourceDatabase
$pgDir = 'C:/Program Files/PostgreSQL/17/bin'
$testDatabase = 'webapolice_catalogos_' + [Guid]::NewGuid().ToString('N')
$schemaFile = Join-Path $env:TEMP ($testDatabase + '-schema.sql')
$catalogFile = Join-Path $env:TEMP ($testDatabase + '-catalog.sql')
$created = $false
try {
    & (Join-Path $pgDir 'pg_dump.exe') --schema-only --no-owner --no-privileges --file $schemaFile
    if ($LASTEXITCODE) { throw 'Falha ao obter a estrutura da base de desenvolvimento.' }
    # Apenas histórico técnico e catálogo de permissões. Não copia clientes nem usuários.
    & (Join-Path $pgDir 'pg_dump.exe') --data-only --no-owner --no-privileges --table '*.__*' --table seguranca.modulo --table seguranca.recurso --table seguranca.permissao --file $catalogFile
    if ($LASTEXITCODE) { throw 'Falha ao obter o histórico técnico.' }
    & (Join-Path $pgDir 'psql.exe') -X --no-password -v ON_ERROR_STOP=1 -c "CREATE DATABASE $testDatabase"
    if ($LASTEXITCODE) { throw 'Falha ao criar a base isolada.' }
    $created = $true
    $env:PGDATABASE = $testDatabase
    & (Join-Path $pgDir 'psql.exe') -X --no-password -v ON_ERROR_STOP=1 --quiet --file $schemaFile
    if ($LASTEXITCODE) { throw 'Falha ao preparar a estrutura isolada.' }
    & (Join-Path $pgDir 'psql.exe') -X --no-password -v ON_ERROR_STOP=1 --quiet --file $catalogFile
    if ($LASTEXITCODE) { throw 'Falha ao preparar o catálogo isolado.' }
    $connection.set_Item('Database', $testDatabase)
    $env:WEBAPOLICE_CATALOGOS_TEST_CONNECTION = $connection.get_ConnectionString()
    Push-Location $backendDir
    try {
        & dotnet build tests/WebApolice.Integration.Tests --no-restore -p:OutputPath=bin/Catalogos/ -v quiet -clp:ErrorsOnly
        if ($LASTEXITCODE) { throw "Falha na compilação dos testes." }
        & dotnet test tests/WebApolice.Integration.Tests --no-build --no-restore -p:OutputPath=bin/Catalogos/ --filter FullyQualifiedName~CatalogosWorkflowTests --logger "console;verbosity=normal" -v minimal
        if ($LASTEXITCODE) { throw 'Os testes dos cadastros falharam.' }
    } finally { Pop-Location }
} finally {
    Remove-Item Env:WEBAPOLICE_CATALOGOS_TEST_CONNECTION -ErrorAction SilentlyContinue
    if ($created) {
        if ($testDatabase -notmatch '^webapolice_catalogos_[a-f0-9]{32}$' -or $testDatabase -eq $sourceDatabase) { throw 'Nome de base temporária inválido para limpeza.' }
        $env:PGDATABASE = $sourceDatabase
        & (Join-Path $pgDir 'psql.exe') -X --no-password -v ON_ERROR_STOP=1 -c "DROP DATABASE $testDatabase WITH (FORCE)"
        if ($LASTEXITCODE) { Write-Warning "Limpeza pendente da base isolada: $testDatabase" }
    }
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $schemaFile, $catalogFile -ErrorAction SilentlyContinue
}
