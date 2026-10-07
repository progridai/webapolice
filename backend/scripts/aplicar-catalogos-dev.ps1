$ErrorActionPreference = 'Stop'
$backendDir = Split-Path $PSScriptRoot -Parent
$line = Get-Content -LiteralPath (Join-Path $backendDir '.env.local') | Where-Object { $_ -match '^\s*ConnectionStrings__PostgreSql=' } | Select-Object -First 1
if (-not $line) { throw 'ConnectionStrings__PostgreSql nao configurada.' }
$connection = New-Object System.Data.Common.DbConnectionStringBuilder
$connection.set_ConnectionString($line.Substring($line.IndexOf('=') + 1).Trim().Trim('"').Trim("'"))
if ($connection.get_Item('Database') -ne 'webapolice_teste') { throw 'Este script aplica somente em webapolice_teste.' }
$env:PGHOST = [string]$connection.get_Item('Host')
$env:PGPORT = if ($connection.ContainsKey('Port')) { [string]$connection.get_Item('Port') } else { '5432' }
$env:PGUSER = [string]$connection.get_Item('Username')
$env:PGPASSWORD = [string]$connection.get_Item('Password')
$env:PGDATABASE = [string]$connection.get_Item('Database')
$env:PGCONNECT_TIMEOUT = '10'
$psql = 'C:/Program Files/PostgreSQL/17/bin/psql.exe'
try {
    @'
DO $$ BEGIN
 IF NOT EXISTS(SELECT 1 FROM seguro."__EFMigrationsHistory" WHERE migration_id='20260923232806_RemoverApoliceSubestipulanteDeApoliceVida') THEN
  RAISE EXCEPTION 'Baseline Seguro divergente. Revisar antes de aplicar.';
 END IF;
 IF NOT EXISTS(SELECT 1 FROM seguranca."__EFMigrationsHistory" WHERE migration_id='20261001201500_AdicionarModuloCrm') THEN
  RAISE EXCEPTION 'Baseline Seguranca divergente. Revisar antes de aplicar.';
 END IF;
END $$;
'@ | & $psql -X --no-password -v ON_ERROR_STOP=1 --quiet
    if ($LASTEXITCODE) { throw 'Pre-condicoes nao atendidas.' }
    foreach ($file in @('01-financeiro.sql', '02-seguro.sql', '03-seguranca.sql', '04-premios.sql', '05-plano-modulo.sql', '06-cobertura-compartilhada.sql')) {
        & $psql -X --no-password -v ON_ERROR_STOP=1 --quiet --file (Join-Path $PSScriptRoot "migrations/catalogos/$file")
        if ($LASTEXITCODE) { throw "Falha em $file. Interrompido; scripts sao idempotentes." }
    }
    Write-Host 'Cadastros aplicados em webapolice_teste.'
} finally {
    Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
}
