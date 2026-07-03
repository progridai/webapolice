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
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task GetHealthLive_SemToken_Retorna200_IndependenteDeBanco()
    {
        // Act
        var response = await _client.GetAsync("/api/health/live");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task GetHealthReady_SemBanco_Retorna503ESemDadosSensiveis()
    {
        // Act (banco de testes é dummy, logo não conecta)
        var response = await _client.GetAsync("/api/health/ready");
        
        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Valida que não vaza dados sensíveis (connection string, servidor, usuário)
        Assert.DoesNotContain("localhost", content);
        Assert.DoesNotContain("Port=5432", content);
        Assert.DoesNotContain("Username=test", content);
        Assert.DoesNotContain("Npgsql", content);
        
        // Valida que retorna o JSON esperado
        Assert.Contains("\"status\":\"Unhealthy\"", content);
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
