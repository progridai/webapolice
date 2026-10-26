$ErrorActionPreference = 'Stop'
$backendDir = Split-Path $PSScriptRoot -Parent
# Os testes criam seu próprio PostgreSQL 18.4 efêmero. Não ler .env ou copiar bases existentes.
Push-Location $backendDir
try {
    & dotnet test tests/WebApolice.Integration.Tests --no-restore -c Release -p:OutputPath=bin/Catalogos/ --filter FullyQualifiedName~CatalogosWorkflowTests --logger trx
    if ($LASTEXITCODE) { throw 'Os testes de catálogos falharam.' }
} finally { Pop-Location }
