# Script temporario para inspecionar claims do token JWT emitido pelo Keycloak
# Este script realiza o fluxo PKCE completo e exibe os claims do token
# APENAS PARA USO LOCAL DE DESENVOLVIMENTO - NAO VERSIONAR OUTPUT
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  WebApolice - Inspecao de Claims do Token JWT            " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Carregar variaveis de ambiente a partir do .env
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
        $Buffer = [System.Text.Encoding]::UTF8.GetBytes("<html><body><h2>Token Captured!</h2><p>Feche esta janela.</p></body></html>")
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
            $Parts = $Jwt.Split(".")
            $PayloadBase64 = $Parts[1]
            $PadLength = 4 - ($PayloadBase64.Length % 4)
            if ($PadLength -ne 4) { $PayloadBase64 += "=" * $PadLength }
            $PayloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($PayloadBase64))
            $Claims = $PayloadJson | ConvertFrom-Json

            Write-Host "`n========== CLAIMS DO ACCESS TOKEN (sanitizado) ==========" -ForegroundColor Cyan
            Write-Host "iss (Issuer):           $($Claims.iss)"
            Write-Host "sub (Subject):          $($Claims.sub.Substring(0, 8))...[MASKED]"
            Write-Host "azp (Authorized Party): $($Claims.azp)"
            Write-Host "aud (Audience):         $($Claims.aud | ConvertTo-Json -Compress)"
            Write-Host "aud tipo:               $($Claims.aud.GetType().Name)"
            Write-Host "preferred_username:     $($Claims.preferred_username)"
            Write-Host "realm_access.roles:     $($Claims.realm_access.roles | ConvertTo-Json -Compress)"
            Write-Host "exp (Expiration):       $($Claims.exp)"
            Write-Host "iat (Issued At):        $($Claims.iat)"
            Write-Host "==========================================================" -ForegroundColor Cyan

            # Verificar se webapolice-api esta no aud
            $audArray = @($Claims.aud)
            if ($audArray -contains "webapolice-api") {
                Write-Host "[+] 'webapolice-api' PRESENTE no claim 'aud'!" -ForegroundColor Green
            } else {
                Write-Host "[-] 'webapolice-api' NAO encontrado no claim 'aud'!" -ForegroundColor Red
                Write-Host "    Sera necessario configurar um audience mapper." -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "[-] Timeout: Nenhum redirecionamento recebido." -ForegroundColor Red
    }
} finally {
    $Listener.Stop()
    Write-Host "`nListener finalizado."
}
