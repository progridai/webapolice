using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace WebApolice.Api.Tests;

/// <summary>
/// Testes de integração para validação do token JWT.
/// Verifica que a API aceita tokens válidos e rejeita tokens inválidos com 401.
///
/// Nota: Os testes não dependem do Keycloak local estar em execução.
/// Os tokens são gerados com chave RSA controlada pelo projeto de testes.
/// </summary>
public sealed class ValidacaoTokenTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public ValidacaoTokenTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Token_Valido_EAceito()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_ComIssuerIncorreto_EhRejeitado()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(
            issuer: "http://issuer-errado.com/realms/outro",
            roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_ComAudienciaIncorreta_EhRejeitado()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(
            audience: "audience-errada",
            roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_Expirado_EhRejeitado()
    {
        // Token com expiração no passado
        var token = ApiTestFactory.GerarTokenDeTeste(
            expiracao: DateTime.UtcNow.AddMinutes(-10),
            roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_SemAssinaturaValida_EhRejeitado()
    {
        // Token com alg=none (sem assinatura)
        var token = ApiTestFactory.GerarTokenDeTeste(semAssinatura: true, roles: ["admin"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_ComAudienciaComoStringUnica_EhAceito()
    {
        // Valida que o formato 'aud' como string única funciona (como Keycloak pode emitir)
        var token = ApiTestFactory.GerarTokenDeTeste(
            audience: ApiTestFactory.AudienciaDeTeste,
            roles: ["operador"]);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Token_ComValorRuim_EhRejeitado()
    {
        // Token completamente inválido (não é um JWT)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "isso-nao-e-um-jwt-valido");

        var resposta = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
