$ErrorActionPreference = "Stop"

$BackendPath = Resolve-Path (Join-Path $PSScriptRoot "..")
$ApiPath = Join-Path $BackendPath "src\WebApolice.Api"

# Carrega e valida configurações compartilhadas
. (Join-Path $PSScriptRoot "load-env.ps1")

# Validações extras
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error "O SDK do .NET nao foi encontrado. Verifique a instalacao."
    exit 1
}

if (-not (Test-Path $ApiPath)) {
    Write-Error "Projeto nao encontrado: $ApiPath"
    exit 1
}

# 5. Iniciar API
Write-Host "Iniciando a API..." -ForegroundColor Cyan
Set-Location $BackendPath
dotnet run --project "src\WebApolice.Api"
