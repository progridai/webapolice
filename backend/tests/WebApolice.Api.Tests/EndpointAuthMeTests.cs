using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WebApolice.Api.Tests;

/// <summary>
/// Testes de integração para o endpoint GET /api/auth/me.
/// Verifica autenticação, retorno de claims corretos e ausência de dados sensíveis.
/// </summary>
public sealed class EndpointAuthMeTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public EndpointAuthMeTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_SemToken_Retorna401()
    {
        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task GetMe_SemToken_RetornaContentTypeProblemJson()
    {
        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.NotNull(resposta.Content.Headers.ContentType);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMe_SemToken_RetornaProblemDetailsComStatus401()
    {
        var resposta = await _client.GetAsync("/api/auth/me");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(401, conteudo.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(conteudo.GetProperty("title").GetString()));
    }

    [Fact]
    public async Task GetMe_ComTokenValido_Retorna200()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task GetMe_ComTokenValido_RetornaSub()
    {
        const string subEsperado = "meu-sub-de-teste-12345";
        var token = ApiTestFactory.GerarTokenDeTeste(sub: subEsperado, roles: ["operador"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(subEsperado, conteudo.GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetMe_ComTokenValido_RetornaPreferredUsername()
    {
        const string usernameEsperado = "rodrigo.silva";
        var token = ApiTestFactory.GerarTokenDeTeste(preferredUsername: usernameEsperado, roles: ["gestor"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(usernameEsperado, conteudo.GetProperty("usuario").GetString());
    }

    [Fact]
    public async Task GetMe_ComTokenValido_RetornaRolesMapeadas()
    {
        var rolesEsperadas = new[] { "admin", "gestor" };
        var token = ApiTestFactory.GerarTokenDeTeste(roles: rolesEsperadas);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        var rolesRetornadas = conteudo.GetProperty("roles")
            .EnumerateArray()
            .Select(r => r.GetString()!)
            .ToList();

        foreach (var role in rolesEsperadas)
        {
            Assert.Contains(role, rolesRetornadas);
        }
    }

    [Fact]
    public async Task GetMe_ComTokenValido_NaoRetornaToken()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");
        var json = await resposta.Content.ReadAsStringAsync();

        // O token JWT começa com "eyJ" — verificar que não está no payload da resposta
        Assert.DoesNotContain("eyJ", json);
        Assert.DoesNotContain("access_token", json);
        Assert.DoesNotContain("refresh_token", json);
    }

    [Fact]
    public async Task GetMe_ComTokenValido_NaoRetornaClaimsSensiveis()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");
        var json = await resposta.Content.ReadAsStringAsync();

        // Verificar que claims internas do JWT não estão expostas
        Assert.DoesNotContain("realm_access", json);
        Assert.DoesNotContain("azp", json);
        Assert.DoesNotContain("resource_access", json);
    }
}
