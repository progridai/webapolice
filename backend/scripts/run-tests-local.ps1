$ErrorActionPreference = "Stop"

$BackendPath = Resolve-Path (Join-Path $PSScriptRoot "..")

# Carrega e valida configurações compartilhadas
. (Join-Path $PSScriptRoot "load-env.ps1")

# Validações extras
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error "O SDK do .NET nao foi encontrado. Verifique a instalacao."
    exit 1
}

Write-Host "Realizando Build..." -ForegroundColor Cyan
Set-Location $BackendPath
$build = Start-Process "dotnet" -ArgumentList "build" -Wait -NoNewWindow -PassThru
if ($build.ExitCode -ne 0) {
    Write-Error "O build falhou."
    exit $build.ExitCode
}

Write-Host "Executando testes unitarios..." -ForegroundColor Cyan
$unit = Start-Process "dotnet" -ArgumentList "test tests/WebApolice.Unit.Tests" -Wait -NoNewWindow -PassThru
$unitExit = $unit.ExitCode

Write-Host "Executando testes de integracao de Seguranca..." -ForegroundColor Cyan
$integ = Start-Process "dotnet" -ArgumentList "test tests/WebApolice.Integration.Tests --filter `"FullyQualifiedName~Seguranca`"" -Wait -NoNewWindow -PassThru
$integExit = $integ.ExitCode

Write-Host "Resumo dos Testes:" -ForegroundColor Cyan
if ($unitExit -eq 0 -and $integExit -eq 0) {
    Write-Host "Todos os testes passaram com sucesso!" -ForegroundColor Green
} else {
    Write-Host "Alguns testes falharam." -ForegroundColor Red
}

if ($unitExit -ne 0) { exit $unitExit }
if ($integExit -ne 0) { exit $integExit }
exit 0
