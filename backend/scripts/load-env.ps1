$BackendPath = Resolve-Path (Join-Path $PSScriptRoot "..")
$EnvFilePath = Join-Path $BackendPath ".env.local"

# Configurações públicas obrigatórias
$RequiredKeys = @(
    "ASPNETCORE_ENVIRONMENT",
    "ConnectionStrings__PostgreSql",
    "ConnectionStrings__PostgreSqlTestes",
    "KeycloakAdmin__BaseUrl",
    "KeycloakAdmin__Realm",
    "KeycloakAdmin__ClientId",
    "KeycloakAdmin__ClientSecret"
)

$PublicDefaults = @{
    "ASPNETCORE_ENVIRONMENT" = "Development"
    "KeycloakAdmin__BaseUrl" = "https://auth.bravida.com.br"
    "KeycloakAdmin__Realm" = "webapolice"
    "KeycloakAdmin__ClientId" = "webapolice-backend-admin"
}

# 1. Carregar variáveis do .env.local em um dicionário
$EnvVars = @{}

if (Test-Path $EnvFilePath) {
    $Lines = Get-Content $EnvFilePath
    foreach ($Line in $Lines) {
        $Trimmed = $Line.Trim()
        if (-not [string]::IsNullOrWhiteSpace($Trimmed) -and -not $Trimmed.StartsWith("#")) {
            $Index = $Trimmed.IndexOf("=")
            if ($Index -gt 0) {
                $Key = $Trimmed.Substring(0, $Index).Trim()
                $Value = $Trimmed.Substring($Index + 1).Trim()
                $EnvVars[$Key] = $Value
            }
        }
    }
}

# 2. Solicitar valores ausentes
$Updated = $false

foreach ($Key in $RequiredKeys) {
    if (-not $EnvVars.ContainsKey($Key) -or [string]::IsNullOrWhiteSpace($EnvVars[$Key])) {
        
        if ($PublicDefaults.ContainsKey($Key)) {
            $EnvVars[$Key] = $PublicDefaults[$Key]
            $Updated = $true
        }
        else {
            if ($Key -eq "ConnectionStrings__PostgreSql") {
                Write-Host "Configuração ausente: $Key" -ForegroundColor Yellow
                $Value = Read-Host "Informe a connection string do PostgreSQL de desenvolvimento"
                if ([string]::IsNullOrWhiteSpace($Value)) {
                    Write-Error "A connection string é obrigatória."
                    exit 1
                }
                $EnvVars[$Key] = $Value
                $Updated = $true
            }
            elseif ($Key -eq "ConnectionStrings__PostgreSqlTestes") {
                Write-Host "Configuração ausente: $Key" -ForegroundColor Yellow
                $Value = Read-Host "Informe a connection string do PostgreSQL para os testes de integração"
                if ([string]::IsNullOrWhiteSpace($Value)) {
                    Write-Error "A connection string de testes é obrigatória."
                    exit 1
                }
                $EnvVars[$Key] = $Value
                $Updated = $true
            }
            elseif ($Key -eq "KeycloakAdmin__ClientSecret") {
                Write-Host "Configuração ausente: $Key" -ForegroundColor Yellow
                $SecureValue = Read-Host "Informe o Client Secret do Keycloak" -AsSecureString
                if ($null -eq $SecureValue) {
                    Write-Error "O Client Secret é obrigatório."
                    exit 1
                }
                
                $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureValue)
                $Value = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
                [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
                
                if ([string]::IsNullOrWhiteSpace($Value)) {
                    Write-Error "O Client Secret é obrigatório."
                    exit 1
                }
                
                $EnvVars[$Key] = $Value
                $Updated = $true
            }
        }
    }
}

# Validação do banco compartilhado temporário
$TestesPermitirBancoCompartilhadoKey = "Testes__PermitirBancoCompartilhado"
if ($EnvVars["ConnectionStrings__PostgreSql"] -eq $EnvVars["ConnectionStrings__PostgreSqlTestes"]) {
    if (-not $EnvVars.ContainsKey($TestesPermitirBancoCompartilhadoKey) -or $EnvVars[$TestesPermitirBancoCompartilhadoKey] -ne "true") {
        Write-Host "ATENÇÃO: o banco de testes está apontando para o mesmo banco de desenvolvimento." -ForegroundColor Yellow
        Write-Host "Utilize esta configuração somente enquanto a base não possuir dados importantes." -ForegroundColor Yellow
        $Confirm = Read-Host "Digite CONFIRMAR para continuar"
        if ($Confirm -cne "CONFIRMAR") {
            Write-Error "Operação cancelada."
            exit 1
        }
        $EnvVars[$TestesPermitirBancoCompartilhadoKey] = "true"
        $Updated = $true
    }
} else {
    if (-not $EnvVars.ContainsKey($TestesPermitirBancoCompartilhadoKey) -or $EnvVars[$TestesPermitirBancoCompartilhadoKey] -ne "false") {
        $EnvVars[$TestesPermitirBancoCompartilhadoKey] = "false"
        $Updated = $true
    }
}

# 3. Salvar o .env.local se houve atualizações
if ($Updated) {
    $FileContent = @()
    foreach ($Key in $RequiredKeys) {
        $FileContent += "$Key=$($EnvVars[$Key])"
    }
    $FileContent += "$TestesPermitirBancoCompartilhadoKey=$($EnvVars[$TestesPermitirBancoCompartilhadoKey])"
    
    Set-Content -Path $EnvFilePath -Value $FileContent -Encoding UTF8
    Write-Host "Arquivo .env.local gerado/atualizado com sucesso." -ForegroundColor Green
}

# 4. Validar e setar no processo atual
foreach ($Key in $RequiredKeys) {
    if (-not $EnvVars.ContainsKey($Key) -or [string]::IsNullOrWhiteSpace($EnvVars[$Key])) {
        Write-Error "Configuração obrigatória ausente: $Key"
        exit 1
    }
    [Environment]::SetEnvironmentVariable($Key, $EnvVars[$Key], [EnvironmentVariableTarget]::Process)
}
[Environment]::SetEnvironmentVariable($TestesPermitirBancoCompartilhadoKey, $EnvVars[$TestesPermitirBancoCompartilhadoKey], [EnvironmentVariableTarget]::Process)
