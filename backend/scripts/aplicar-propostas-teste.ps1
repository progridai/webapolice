param([switch]$Aplicar, [switch]$Preenchimento, [switch]$Confirmacao, [switch]$Agendamentos, [switch]$Encerramento)
if (([int]$Preenchimento.IsPresent + [int]$Confirmacao.IsPresent + [int]$Agendamentos.IsPresent + [int]$Encerramento.IsPresent) -gt 1) { throw 'Selecione somente uma etapa por execução.' }
$ErrorActionPreference = 'Stop'
$backendDir = Split-Path $PSScriptRoot -Parent
$line = Get-Content -LiteralPath (Join-Path $backendDir '.env.local') | Where-Object { $_ -match '^ConnectionStrings__PostgreSqlTestes=' } | Select-Object -First 1
if (-not $line) { throw 'Configuração específica de testes não encontrada.' }
$connection = New-Object System.Data.Common.DbConnectionStringBuilder
try { $connection.set_ConnectionString($line.Substring($line.IndexOf('=') + 1).Trim().Trim('"').Trim("'")) }
catch { throw 'Configuração de testes inválida.' }
if ($connection['Database'] -ne 'webapolice_teste') { throw 'Este instalador aceita somente webapolice_teste.' }
$psql = 'C:/Program Files/PostgreSQL/17/bin/psql.exe'
if (-not (Test-Path -LiteralPath $psql)) { throw 'Cliente PostgreSQL não encontrado no caminho homologado.' }
$keys = @('PGHOST','PGPORT','PGDATABASE','PGUSER','PGPASSWORD','PGCONNECT_TIMEOUT','PGOPTIONS','PGCLIENTENCODING')
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
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'diagnosticar-propostas.sql')
    if ($LASTEXITCODE) { throw 'Diagnóstico não concluído. Nenhuma migration foi aplicada.' }
    if (-not $Aplicar) { return }
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Docker não encontrado para backup de estrutura com PostgreSQL 18.4.' }
    $evidencias = Join-Path (Split-Path $backendDir -Parent) ($(if ($Encerramento) { 'scratch/propostas-encerramento-' } elseif ($Agendamentos) { 'scratch/propostas-agendamentos-' } elseif ($Confirmacao) { 'scratch/propostas-confirmacao-' } elseif ($Preenchimento) { 'scratch/propostas-preenchimento-' } else { 'scratch/propostas-aplicacao-' }) + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path $evidencias -Force | Out-Null
    & docker run --rm --env PGHOST --env PGPORT --env PGDATABASE --env PGUSER --env PGPASSWORD --env PGCONNECT_TIMEOUT --env PGOPTIONS --mount "type=bind,source=$evidencias,target=/backup" postgres:18.4 pg_dump --schema-only --no-owner --no-privileges --file /backup/estrutura-anterior.sql
    if ($LASTEXITCODE) { throw 'Backup de estrutura não concluído.' }
    $env:PGOPTIONS = ''
    $migration = if ($Encerramento) { 'migrations/propostas/08-encerramento-descarte.sql' } elseif ($Agendamentos) { 'migrations/propostas/07-gestao-agendamentos.sql' } elseif ($Confirmacao) { 'migrations/propostas/06-confirmacao-vigencia.sql' } elseif ($Preenchimento) { 'migrations/propostas/05-preenchimento-rascunho.sql' } else { 'migrations/propostas/00-instalar-teste.sql' }
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot $migration)
    if ($LASTEXITCODE) { throw 'Instalação transacional recusada. Não iniciar a API nova.' }
    # Script integral, em sua própria transação, sem normalização histórica.
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 -c "SET client_min_messages='warning'; SET webapolice.title_case_normalizar_existentes='off'" --file (Join-Path $PSScriptRoot 'padronizar-title-case.sql')
    if ($LASTEXITCODE) { throw 'Migrations aplicadas, mas os triggers Title Case precisam ser corrigidos antes de iniciar a API.' }
    $env:PGOPTIONS = '-c default_transaction_read_only=on'
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'diagnosticar-propostas.sql')
    if ($LASTEXITCODE) { throw 'A verificação posterior não foi concluída.' }
    & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'verificar-propostas-instaladas.sql')
    if ($LASTEXITCODE) { throw 'A proteção das novas gravações precisa ser revisada.' }
    if ($Encerramento) {
        & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'verificar-encerramento-propostas.sql')
        if ($LASTEXITCODE) { throw 'Verificacao do encerramento nao concluida.' }
        Write-Host 'Duas migrations especificas de encerramento aplicadas; registros, historico e concessoes preservados.'
    } elseif ($Agendamentos) {
        & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'verificar-agendamentos-propostas.sql')
        if ($LASTEXITCODE) { throw 'Verificação dos agendamentos não concluída.' }
        Write-Host 'Duas migrations específicas de agendamentos aplicadas; dados, histórico e concessões preservados.'
    } elseif ($Confirmacao) {
        & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'verificar-confirmacao-propostas.sql')
        if ($LASTEXITCODE) { throw 'Verificação da confirmação não concluída.' }
        Write-Host 'Duas migrations específicas de confirmação aplicadas; dados anteriores, catálogos existentes e concessões preservados.'
    } elseif ($Preenchimento) {
        & $psql -X --no-password --quiet -v ON_ERROR_STOP=1 --file (Join-Path $PSScriptRoot 'verificar-preenchimento-propostas.sql')
        if ($LASTEXITCODE) { throw 'Verificação do preenchimento não concluída.' }
        Write-Host 'Migration específica de preenchimento aplicada; dados anteriores e concessões preservados.'
    } else {
        Write-Host 'Quatro migrations específicas aplicadas; Title Case atualizado sem reprocessar histórico. Nenhum perfil recebeu permissões.'
    }
    Write-Host "Backup somente da estrutura: $evidencias"
} finally {
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key], 'Process') }
}
