using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Keycloak;

public class KeycloakUsuariosAdminClient : IKeycloakUsuariosAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakUsuariosAdminClient(HttpClient httpClient, IOptions<KeycloakAdminOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        
        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/'));
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var tokenUrl = $"/realms/{_options.Realm}/protocol/openid-connect/token";
        
        var requestContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret)
        });

        var response = await _httpClient.PostAsync(tokenUrl, requestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        
        _cachedToken = document.RootElement.GetProperty("access_token").GetString();
        var expiresIn = document.RootElement.GetProperty("expires_in").GetInt32();
        
        // Retira 10 segundos da expiração real por segurança
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 10);

        return _cachedToken!;
    }

    private async Task SetAuthorizationHeaderAsync(CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<string> CriarUsuarioAsync(string username, string email, string nome, bool ativo, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var (firstName, lastName) = SplitNome(nome);

        var payload = new
        {
            username = username,
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = ativo
        };

        var usersUrl = $"/admin/realms/{_options.Realm}/users";
        var response = await _httpClient.PostAsJsonAsync(usersUrl, payload, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("Usuário já existe no Keycloak.");
        }

        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(location))
        {
            throw new InvalidOperationException("Keycloak não retornou o Location header com o ID do usuário.");
        }

        var segments = location.Split('/');
        return segments.Last();
    }

    public async Task DefinirSenhaTemporariaAsync(string keycloakSub, string senhaTemporaria, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var payload = new
        {
            type = "password",
            value = senhaTemporaria,
            temporary = true
        };

        var resetPasswordUrl = $"/admin/realms/{_options.Realm}/users/{keycloakSub}/reset-password";
        var response = await _httpClient.PutAsJsonAsync(resetPasswordUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task AtualizarUsuarioAsync(string keycloakSub, string email, string nome, bool ativo, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var (firstName, lastName) = SplitNome(nome);

        var payload = new
        {
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = ativo
        };

        var userUrl = $"/admin/realms/{_options.Realm}/users/{keycloakSub}";
        var response = await _httpClient.PutAsJsonAsync(userUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task InativarUsuarioAsync(string keycloakSub, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var payload = new { enabled = false };

        var userUrl = $"/admin/realms/{_options.Realm}/users/{keycloakSub}";
        var response = await _httpClient.PutAsJsonAsync(userUrl, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ExisteUsernameAsync(string username, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var urlUsername = $"/admin/realms/{_options.Realm}/users?username={username}&exact=true";
        var responseUser = await _httpClient.GetAsync(urlUsername, cancellationToken);
        responseUser.EnsureSuccessStatusCode();
        var contentUser = await responseUser.Content.ReadAsStringAsync(cancellationToken);
        
        return contentUser != "[]";
    }

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var urlEmail = $"/admin/realms/{_options.Realm}/users?email={email}&exact=true";
        var responseEmail = await _httpClient.GetAsync(urlEmail, cancellationToken);
        responseEmail.EnsureSuccessStatusCode();
        var contentEmail = await responseEmail.Content.ReadAsStringAsync(cancellationToken);
        
        return contentEmail != "[]";
    }

    public async Task<KeycloakUsuarioRecord?> ObterUsuarioPorSubAsync(string keycloakSub, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var url = $"/admin/realms/{_options.Realm}/users/{keycloakSub}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        return new KeycloakUsuarioRecord(
            Id: root.GetProperty("id").GetString()!,
            Username: root.GetProperty("username").GetString()!,
            Email: root.TryGetProperty("email", out var el) ? el.GetString() ?? "" : "",
            FirstName: root.TryGetProperty("firstName", out var fn) ? fn.GetString() ?? "" : "",
            LastName: root.TryGetProperty("lastName", out var ln) ? ln.GetString() ?? "" : "",
            Enabled: root.TryGetProperty("enabled", out var en) && en.GetBoolean()
        );
    }

    public async Task RemoverUsuarioAsync(string keycloakSub, CancellationToken cancellationToken)
    {
        await SetAuthorizationHeaderAsync(cancellationToken);

        var url = $"/admin/realms/{_options.Realm}/users/{keycloakSub}";
        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }
    }

    private static (string firstName, string lastName) SplitNome(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto)) return ("", "");
        
        var parts = nomeCompleto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : "";
        return (firstName, lastName);
    }
}
