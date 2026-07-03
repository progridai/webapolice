# Script de Teste Interativo para Validação do Fluxo OIDC PKCE no Keycloak
# Caminho: infrastructure/keycloak/tests/test-pkce.ps1
$ErrorActionPreference = "Stop"

Write-Host "=========================================================="
Write-Host "  WebApolice - Validador do Fluxo OIDC PKCE (Keycloak)   "
Write-Host "=========================================================="

# 1. Carregar variáveis de ambiente a partir do .env
$EnvFile = Join-Path $PSScriptRoot "../../../.env"
if (Test-Path $EnvFile) {
    Write-Host "[+] Carregando variaveis do arquivo .env..."
    Get-Content $EnvFile | ForEach-Object {
        $Line = $_.Trim()
        if ($Line -and -not $Line.StartsWith("#") -and $Line.Contains("=")) {
            $Key, $Value = $Line.Split("=", 2)
            $Key = $Key.Trim()
            $Value = $Value.Trim().Trim('"').Trim("'")
            [System.Environment]::SetEnvironmentVariable($Key, $Value)
        }
    }
}

# Obter configurações das variáveis de ambiente
$BaseUrl = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_URL")
if (-not $BaseUrl) { $BaseUrl = "http://127.0.0.1:8080" }
$Realm = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_REALM")
if (-not $Realm) { $Realm = "webapolice" }
$ClientId = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_WEB_CLIENT_ID")
if (-not $ClientId) { $ClientId = "webapolice-web" }
$RedirectUri = "http://127.0.0.1:5173/"

Write-Host "[+] Base URL: $BaseUrl"
Write-Host "[+] Realm: $Realm"
Write-Host "[+] Client ID: $ClientId"

# 2. Gerar chaves PKCE de forma dinâmica
# Verifier: String aleatória de alta entropia
$RandomBytes = New-Object Byte[] 32
$Rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$Rng.GetBytes($RandomBytes)
$CodeVerifier = [System.Convert]::ToBase64String($RandomBytes).Split("=")[0].Replace('+', '-').Replace('/', '_')

# Challenge: SHA256 do Verifier codificado em Base64Url
$Sha256 = [System.Security.Cryptography.SHA256]::Create()
$HashBytes = $Sha256.ComputeHash([System.Text.Encoding]::ASCII.GetBytes($CodeVerifier))
$CodeChallenge = [System.Convert]::ToBase64String($HashBytes).Split("=")[0].Replace('+', '-').Replace('/', '_')

# 3. Montar a URL de Autorização
$AuthUrl = "$BaseUrl/realms/$Realm/protocol/openid-connect/auth?response_type=code&client_id=$ClientId&redirect_uri=$RedirectUri&code_challenge=$CodeChallenge&code_challenge_method=S256&scope=openid"

Write-Host "`n[Instrucao] Abra a seguinte URL no seu navegador para realizar o login:" -ForegroundColor Yellow
Write-Host $AuthUrl -ForegroundColor Cyan
Write-Host "`nIniciando HTTP Listener local na porta 5173 para capturar o redirecionamento..."

# 4. Iniciar o HTTP Listener temporário
$Listener = New-Object System.Net.HttpListener
$Listener.Prefixes.Add($RedirectUri)
$Listener.Start()

try {
    Write-Host "Aguardando redirecionamento na porta 5173 (timeout em 60 segundos)..."
    $AsyncResult = $Listener.BeginGetContext($null, $null)
    # Aguardar até 60 segundos por uma resposta
    if ($AsyncResult.AsyncWaitHandle.WaitOne(60000)) {
        $Context = $Listener.EndGetContext($AsyncResult)
        $Request = $Context.Request
        $Url = $Request.Url.OriginalString
        
        # Enviar resposta HTTP de sucesso ao browser
        $Response = $Context.Response
        $Buffer = [System.Text.Encoding]::UTF8.GetBytes("<html><body><h2>PKCE Test Capture Success</h2><p>Pode fechar esta janela. O terminal capturou o codigo.</p></body></html>")
        $Response.ContentLength64 = $Buffer.Length
        $Response.OutputStream.Write($Buffer, 0, $Buffer.Length)
        $Response.OutputStream.Close()

        # Extrair código da URL
        $CodeMatch = [regex]::Match($Url, 'code=([^&]+)')
        if ($CodeMatch.Success) {
            $AuthCode = $CodeMatch.Groups[1].Value
            Write-Host "[+] Codigo de autorizacao capturado com sucesso!"
            
            # 5. Executar teste com verifier INCORRETO (Rejeição esperada)
            Write-Host "`n[Teste 1] Trocando o codigo com um code_verifier INCORRETO (Rejeicao Esperada)..."
            $TokenUrl = "$BaseUrl/realms/$Realm/protocol/openid-connect/token"
            $BodyBad = @{
                grant_type = "authorization_code"
                client_id = $ClientId
                code = $AuthCode
                redirect_uri = $RedirectUri
                code_verifier = "wrong_code_verifier_value_12345678901234567890"
            }
            
            try {
                $ResBad = Invoke-RestMethod -Uri $TokenUrl -Method Post -Body $BodyBad
                Write-Host "[-] ERRO CRITICO: O Keycloak aceitou o code_verifier incorreto!" -ForegroundColor Red
            } catch {
                Write-Host "[+] SUCESSO: Keycloak rejeitou a troca com o verifier incorreto conforme o esperado!" -ForegroundColor Green
                Write-Host "  Erro retornado: $_"
            }

            # Nota: Como o código é de uso único, após a tentativa com erro ele foi invalidado pelo Keycloak.
            # Para testar a troca bem-sucedida, é necessário gerar um novo fluxo.
            Write-Host "`n[Teste 2] Para demonstrar a troca BEM-SUCEDIDA com o verifier correto," -ForegroundColor Yellow
            Write-Host "por favor abra a URL de login novamente no navegador e logue-se:" -ForegroundColor Yellow
            Write-Host $AuthUrl -ForegroundColor Cyan
            
            $AsyncResult2 = $Listener.BeginGetContext($null, $null)
            if ($AsyncResult2.AsyncWaitHandle.WaitOne(60000)) {
                $Context2 = $Listener.EndGetContext($AsyncResult2)
                $Request2 = $Context2.Request
                $Url2 = $Request2.Url.OriginalString
                
                $Response2 = $Context2.Response
                $Buffer2 = [System.Text.Encoding]::UTF8.GetBytes("<html><body><h2>PKCE Test Capture Success</h2><p>Pode fechar esta janela. O terminal capturou o novo codigo.</p></body></html>")
                $Response2.ContentLength64 = $Buffer2.Length
                $Response2.OutputStream.Write($Buffer2, 0, $Buffer2.Length)
                $Response2.OutputStream.Close()

                $CodeMatch2 = [regex]::Match($Url2, 'code=([^&]+)')
                if ($CodeMatch2.Success) {
                    $AuthCode2 = $CodeMatch2.Groups[1].Value
                    Write-Host "[+] Novo codigo de autorizacao capturado!"
                    
                    Write-Host "`n[Teste 2] Trocando o novo codigo com o code_verifier CORRETO..."
                    $BodyGood = @{
                        grant_type = "authorization_code"
                        client_id = $ClientId
                        code = $AuthCode2
                        redirect_uri = $RedirectUri
                        code_verifier = $CodeVerifier
                    }
                    
                    try {
                        $ResGood = Invoke-RestMethod -Uri $TokenUrl -Method Post -Body $BodyGood
                        Write-Host "[+] SUCESSO: Tokens gerados com sucesso com o verifier correto!" -ForegroundColor Green
                        Write-Host "  Access Token JWT gerado (sanitizado):"
                        $Jwt = $ResGood.access_token
                        $PayloadBase64 = $Jwt.Split(".")[1]
                        $PadLength = 4 - ($PayloadBase64.Length % 4)
                        if ($PadLength -ne 4) { $PayloadBase64 += "=" * $PadLength }
                        $PayloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($PayloadBase64))
                        
                        # Filtrar apenas claims não sensíveis para exibição
                        Write-Host "  Payload Claims (sanitizado):" -ForegroundColor Cyan
                        $Claims = $PayloadJson | ConvertFrom-Json
                        Write-Host "    Issuer (iss): $($Claims.iss)"
                        Write-Host "    Client ID (azp): $($Claims.azp)"
                        Write-Host "    Subject ID (sub): $($Claims.sub)"
                        Write-Host "    Username: $($Claims.preferred_username)"
                        Write-Host "    Name: $($Claims.name)"
                        Write-Host "    Email: $($Claims.email)"
                        Write-Host "    Realm Roles: $($Claims.realm_access.roles -join ', ')"
                    } catch {
                        Write-Host "[-] ERRO: Falha ao trocar codigo com o verifier correto." -ForegroundColor Red
                        Write-Host $_.Exception.Message
                    }
                }
            } else {
                Write-Host "[-] Timeout: Nenhum segundo redirecionamento recebido." -ForegroundColor Red
            }
        }
    } else {
        Write-Host "[-] Timeout: Nenhum redirecionamento recebido dentro de 60 segundos." -ForegroundColor Red
    }
} finally {
    $Listener.Stop()
    Write-Host "`nHTTP Listener finalizado."
}
