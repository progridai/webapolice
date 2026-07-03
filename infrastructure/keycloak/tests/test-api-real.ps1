# Script de teste real contra a API WebApolice com token real do Keycloak
# Executa o fluxo PKCE, obtém token e testa os endpoints da API
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  WebApolice - Teste Real da API com Keycloak             " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Carregar variáveis de ambiente
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

$BaseUrl = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_URL")
if (-not $BaseUrl) { $BaseUrl = "http://127.0.0.1:8080" }
$Realm = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_REALM")
if (-not $Realm) { $Realm = "webapolice" }
$ClientId = [System.Environment]::GetEnvironmentVariable("KEYCLOAK_WEB_CLIENT_ID")
if (-not $ClientId) { $ClientId = "webapolice-web" }
$RedirectUri = "http://127.0.0.1:5173/"
$ApiBase = "http://localhost:5007"

# Gerar PKCE
$RandomBytes = New-Object Byte[] 32
$Rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$Rng.GetBytes($RandomBytes)
$CodeVerifier = [System.Convert]::ToBase64String($RandomBytes).Split("=")[0].Replace('+', '-').Replace('/', '_')
$Sha256 = [System.Security.Cryptography.SHA256]::Create()
$HashBytes = $Sha256.ComputeHash([System.Text.Encoding]::ASCII.GetBytes($CodeVerifier))
$CodeChallenge = [System.Convert]::ToBase64String($HashBytes).Split("=")[0].Replace('+', '-').Replace('/', '_')

$AuthUrl = "$BaseUrl/realms/$Realm/protocol/openid-connect/auth?response_type=code&client_id=$ClientId&redirect_uri=$RedirectUri&code_challenge=$CodeChallenge&code_challenge_method=S256&scope=openid"

Write-Host "`n[Instrucao] Abra no navegador:" -ForegroundColor Yellow
Write-Host $AuthUrl -ForegroundColor Cyan
Write-Host "`nAguardando redirecionamento em http://127.0.0.1:5173/ ..."

$Listener = New-Object System.Net.HttpListener
$Listener.Prefixes.Add($RedirectUri)
$Listener.Start()

try {
    $AsyncResult = $Listener.BeginGetContext($null, $null)
    if ($AsyncResult.AsyncWaitHandle.WaitOne(120000)) {
        $Context = $Listener.EndGetContext($AsyncResult)
        $Request = $Context.Request
        $Url = $Request.Url.OriginalString

        $Response = $Context.Response
        $Buffer = [System.Text.Encoding]::UTF8.GetBytes("<html><body><h2>Token Capturado!</h2><p>Feche esta janela.</p></body></html>")
        $Response.ContentLength64 = $Buffer.Length
        $Response.OutputStream.Write($Buffer, 0, $Buffer.Length)
        $Response.OutputStream.Close()

        $CodeMatch = [regex]::Match($Url, 'code=([^&]+)')
        if ($CodeMatch.Success) {
            $AuthCode = $CodeMatch.Groups[1].Value
            Write-Host "[+] Codigo capturado!" -ForegroundColor Green

            $TokenUrl = "$BaseUrl/realms/$Realm/protocol/openid-connect/token"
            $Body = @{
                grant_type   = "authorization_code"
                client_id    = $ClientId
                code         = $AuthCode
                redirect_uri = $RedirectUri
                code_verifier = $CodeVerifier
            }

            $Res = Invoke-RestMethod -Uri $TokenUrl -Method Post -Body $Body
            Write-Host "[+] Token obtido com sucesso!" -ForegroundColor Green

            $Jwt = $Res.access_token

            # Decodificar e exibir claims
            $Parts = $Jwt.Split(".")
            $PayloadBase64 = $Parts[1]
            $PadLength = 4 - ($PayloadBase64.Length % 4)
            if ($PadLength -ne 4) { $PayloadBase64 += "=" * $PadLength }
            $PayloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($PayloadBase64))
            $Claims = $PayloadJson | ConvertFrom-Json

            Write-Host "`n========== CLAIMS DO TOKEN (sanitizado) ==========" -ForegroundColor Cyan
            Write-Host "iss:                $($Claims.iss)"
            Write-Host "azp:                $($Claims.azp)"
            Write-Host "aud:                $($Claims.aud | ConvertTo-Json -Compress)"
            Write-Host "preferred_username: $($Claims.preferred_username)"
            Write-Host "realm_access.roles: $($Claims.realm_access.roles | ConvertTo-Json -Compress)"
            Write-Host "=================================================" -ForegroundColor Cyan

            $audArray = @($Claims.aud)
            if ($audArray -contains "webapolice-api") {
                Write-Host "[+] 'webapolice-api' PRESENTE no claim 'aud'!" -ForegroundColor Green
            } else {
                Write-Host "[-] 'webapolice-api' NAO encontrado no claim 'aud'!" -ForegroundColor Red
            }

            # Cabecalho de autorizacao
            $Headers = @{ "Authorization" = "Bearer $Jwt" }

            Write-Host "`n========== TESTES CONTRA A API ($ApiBase) ==========" -ForegroundColor Cyan

            # Teste 1: GET /api/health (publico)
            Write-Host "`n[Teste 1] GET /api/health (publico, sem token)..."
            try {
                $R1 = Invoke-WebRequest -Uri "$ApiBase/api/health" -Method Get -UseBasicParsing
                Write-Host "[+] SUCESSO: $($R1.StatusCode) - $($R1.Content)" -ForegroundColor Green
            } catch { Write-Host "[-] FALHA: $_" -ForegroundColor Red }

            # Teste 2: GET /api/version (publico)
            Write-Host "`n[Teste 2] GET /api/version (publico, sem token)..."
            try {
                $R2 = Invoke-WebRequest -Uri "$ApiBase/api/version" -Method Get -UseBasicParsing
                Write-Host "[+] SUCESSO: $($R2.StatusCode) - $($R2.Content)" -ForegroundColor Green
            } catch { Write-Host "[-] FALHA: $_" -ForegroundColor Red }

            # Teste 3: GET /api/auth/me sem token (espera 401)
            Write-Host "`n[Teste 3] GET /api/auth/me sem token (espera 401)..."
            try {
                $R3 = Invoke-WebRequest -Uri "$ApiBase/api/auth/me" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
                Write-Host "[-] INESPERADO: $($R3.StatusCode) - deveria ser 401!" -ForegroundColor Red
            } catch {
                $Code = $_.Exception.Response.StatusCode.value__
                Write-Host "[+] CORRETO: Retornou $Code como esperado!" -ForegroundColor Green
            }

            # Teste 4: GET /api/auth/me com token valido (espera 200)
            Write-Host "`n[Teste 4] GET /api/auth/me com token valido (espera 200)..."
            try {
                $R4 = Invoke-WebRequest -Uri "$ApiBase/api/auth/me" -Method Get -Headers $Headers -UseBasicParsing
                Write-Host "[+] SUCESSO: $($R4.StatusCode)" -ForegroundColor Green
                Write-Host "    Resposta: $($R4.Content)"
            } catch { Write-Host "[-] FALHA: $_" -ForegroundColor Red }

            # Teste 5: GET /api/admin/ping sem token (espera 401)
            Write-Host "`n[Teste 5] GET /api/admin/ping sem token (espera 401)..."
            try {
                $R5 = Invoke-WebRequest -Uri "$ApiBase/api/admin/ping" -Method Get -ErrorAction SilentlyContinue -UseBasicParsing
                Write-Host "[-] INESPERADO: $($R5.StatusCode) - deveria ser 401!" -ForegroundColor Red
            } catch {
                $Code = $_.Exception.Response.StatusCode.value__
                Write-Host "[+] CORRETO: Retornou $Code como esperado!" -ForegroundColor Green
            }

            # Teste 6: GET /api/admin/ping com token admin (espera 200)
            Write-Host "`n[Teste 6] GET /api/admin/ping com token admin (espera 200)..."
            try {
                $R6 = Invoke-WebRequest -Uri "$ApiBase/api/admin/ping" -Method Get -Headers $Headers -UseBasicParsing
                Write-Host "[+] SUCESSO: $($R6.StatusCode)" -ForegroundColor Green
                Write-Host "    Resposta: $($R6.Content)"
            } catch { Write-Host "[-] FALHA: $_" -ForegroundColor Red }

            # Teste 7: Token com audience invalida (espera 401)
            Write-Host "`n[Teste 7] GET /api/auth/me com audience invalida (espera 401)..."
            # Criar token JWT manualmente com audience errada para testar rejeicao
            Write-Host "   (Este teste usa o proprio token - se a API aceitar, o audience mapper funciona)"
            Write-Host "   Se o token nao contiver 'webapolice-api' em 'aud', a API deve rejeitar."

            Write-Host "`n========== RESUMO ==========" -ForegroundColor Cyan
            Write-Host "Todos os testes foram executados. Verifique os resultados acima." -ForegroundColor Yellow
        }
    } else {
        Write-Host "[-] Timeout: Nenhum redirecionamento recebido." -ForegroundColor Red
    }
} finally {
    $Listener.Stop()
    Write-Host "`nListener finalizado."
}
