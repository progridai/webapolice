param(
    [ValidateSet('Teste', 'Producao')][string]$Ambiente = 'Teste',
    [switch]$Aplicar
)

$ErrorActionPreference = 'Stop'
$backendDirectory = Split-Path $PSScriptRoot -Parent
$configuration = @{}
foreach ($entry in Get-Content -LiteralPath (Join-Path $backendDirectory '.env.local')) {
    if ($entry -match '^([A-Za-z_][A-Za-z_0-9]*)=(.*)$') {
        $configuration[$matches[1]] = $matches[2].Trim().Trim('"').Trim("'")
    }
}

$production = $Ambiente -eq 'Producao'
$connectionKey = if ($production) { 'ConnectionStrings__PostgreSqlProducao' } else { 'ConnectionStrings__PostgreSqlTestes' }
$targetConnection = $configuration[$connectionKey]
if ([string]::IsNullOrWhiteSpace($targetConnection)) { throw 'Configuracao do ambiente ausente.' }
if (-not $production -and $configuration['ConnectionStrings__PostgreSql'] -ne $targetConnection) {
    throw 'A API local nao aponta para a conexao especifica de testes. Nenhuma alteracao foi executada.'
}
if ($configuration['ConnectionStrings__PostgreSqlProducao'] -eq $configuration['ConnectionStrings__PostgreSqlTestes']) {
    throw 'As conexoes de testes e producao coincidem. Nenhuma alteracao foi executada.'
}
$connection = [System.Data.Common.DbConnectionStringBuilder]::new()
try { $connection.set_ConnectionString($targetConnection) }
catch { throw 'Configuracao do ambiente invalida.' }
$expectedDatabase = if ($production) { 'webapolice' } else { 'webapolice_teste' }
if ($connection['Database'] -ne $expectedDatabase) { throw 'Nome do banco incompativel com o ambiente selecionado.' }

$psql = 'C:/Program Files/PostgreSQL/17/bin/psql.exe'
if (-not (Test-Path -LiteralPath $psql)) { throw 'Cliente PostgreSQL homologado nao encontrado.' }
$keys = @('PGHOST', 'PGPORT', 'PGDATABASE', 'PGUSER', 'PGPASSWORD', 'PGCONNECT_TIMEOUT', 'PGOPTIONS', 'PGCLIENTENCODING')
$previous = @{}
foreach ($key in $keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key, 'Process') }

try {
    $env:PGHOST = [string]$connection['Host']
    $env:PGPORT = if ($connection.ContainsKey('Port')) { [string]$connection['Port'] } else { '5432' }
    $env:PGDATABASE = [string]$connection['Database']
    $env:PGUSER = [string]$connection['Username']
    $env:PGPASSWORD = [string]$connection['Password']
    $env:PGCONNECT_TIMEOUT = '10'
    $env:PGCLIENTENCODING = 'UTF8'
    $env:PGOPTIONS = '-c default_transaction_read_only=on'
    $diagnostic = @'
SELECT current_database() AS banco,
       EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='seguranca' AND table_name='usuario' AND column_name='atalhos_home' AND data_type='ARRAY' AND udt_name='_text' AND is_nullable='YES') AS coluna_compativel,
       EXISTS (SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE migration_id='20261117160039_AdicionarAtalhosHomeUsuario') AS migration_instalada;
'@
    $diagnostic | & $psql -X --no-password -v ON_ERROR_STOP=1 --file -
    if ($LASTEXITCODE) { throw 'Diagnostico nao concluido. Nenhuma migration foi aplicada.' }
    if (-not $Aplicar) { return }

    $preconditions = @'
DO $check$
DECLARE registrada boolean; coluna boolean;
BEGIN
    SELECT EXISTS (SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE migration_id='20261117160039_AdicionarAtalhosHomeUsuario') INTO registrada;
    SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='seguranca' AND table_name='usuario' AND column_name='atalhos_home') INTO coluna;
    IF registrada <> coluna THEN RAISE EXCEPTION 'Coluna e historico inconsistentes; instalacao automatica recusada.'; END IF;
    IF coluna AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='seguranca' AND table_name='usuario' AND column_name='atalhos_home' AND data_type='ARRAY' AND udt_name='_text' AND is_nullable='YES') THEN
        RAISE EXCEPTION 'Coluna existente incompativel; instalacao automatica recusada.';
    END IF;
END $check$;
'@
    $preconditions | & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file -
    if ($LASTEXITCODE) { throw 'Pre-condicoes nao atendidas. Nenhuma migration foi aplicada.' }
    $env:PGOPTIONS = '-c lock_timeout=5s -c statement_timeout=30s'
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'migrations/home/atalhos-home.sql')
    if ($LASTEXITCODE) { throw 'Migration especifica de atalhos nao concluida.' }
    $env:PGOPTIONS = '-c default_transaction_read_only=on'
    $diagnostic | & $psql -X --no-password -v ON_ERROR_STOP=1 --file -
    if ($LASTEXITCODE) { throw 'Verificacao posterior nao concluida.' }
    Write-Host 'Migration de atalhos instalada. Nenhum usuario, perfil, concessao ou evidencia historica foi reescrito.'
} finally {
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
}
