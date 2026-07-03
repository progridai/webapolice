using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace WebApolice.Api.Tests;

/// <summary>
/// Testes de integração para endpoints públicos.
/// Estes endpoints devem retornar 200 sem nenhum token de autenticação.
/// </summary>
public sealed class EndpointsPublicosTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public EndpointsPublicosTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_SemToken_Retorna200()
    {
        var resposta = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task GetHealth_SemToken_RetornaStatusHealthy()
    {
        var resposta = await _client.GetAsync("/api/health");
        var conteudo = await resposta.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.NotNull(conteudo);
        Assert.Equal("healthy", conteudo.status);
        Assert.Equal("WebApolice.Api", conteudo.application);
    }

    [Fact]
    public async Task GetVersion_SemToken_Retorna200()
    {
        var resposta = await _client.GetAsync("/api/version");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task GetVersion_SemToken_RetornaInformacoesDeVersao()
    {
        var resposta = await _client.GetAsync("/api/version");
        var conteudo = await resposta.Content.ReadFromJsonAsync<VersionResponse>();

        Assert.NotNull(conteudo);
        Assert.Equal("WebApolice.Api", conteudo.application);
        Assert.Equal("0.1.0", conteudo.version);
        Assert.False(string.IsNullOrWhiteSpace(conteudo.environment));
    }

    private record HealthResponse(string status, string application);
    private record VersionResponse(string application, string version, string environment);
}
