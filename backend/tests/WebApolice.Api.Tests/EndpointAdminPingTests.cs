using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WebApolice.Api.Tests;

/// <summary>
/// Testes de integração para o endpoint GET /api/admin/ping.
/// Verifica controle de acesso com política Admin:
///   - Sem token:              401 Unauthorized
///   - Token sem role admin:   403 Forbidden
///   - Token com role admin:   200 OK
/// </summary>
public sealed class EndpointAdminPingTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public EndpointAdminPingTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminPing_SemToken_Retorna401()
    {
        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task AdminPing_SemToken_RetornaContentTypeProblemJson()
    {
        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.NotNull(resposta.Content.Headers.ContentType);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AdminPing_SemToken_RetornaProblemDetailsComStatus401()
    {
        var resposta = await _client.GetAsync("/api/admin/ping");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(401, conteudo.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task AdminPing_ComTokenSemRoleAdmin_Retorna403()
    {
        // Usuário autenticado mas sem a role 'admin'
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["gestor"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact]
    public async Task AdminPing_ComTokenSemRoleAdmin_RetornaContentTypeProblemJson()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["operador"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.NotNull(resposta.Content.Headers.ContentType);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AdminPing_ComTokenSemRoleAdmin_RetornaProblemDetailsComStatus403()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["gestor"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/admin/ping");
        var conteudo = await resposta.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(403, conteudo.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task AdminPing_ComRoleAdmin_Retorna200()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task AdminPing_ComTokenSemNenhumaRole_Retorna403()
    {
        // Usuário autenticado mas sem nenhuma role
        var token = ApiTestFactory.GerarTokenDeTeste(roles: []);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/admin/ping");

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
