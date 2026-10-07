using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace WebApolice.Api.Tests;

public sealed class CatalogosHttpTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;
    public CatalogosHttpTests(ApiTestFactory factory) => _client = factory.WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.ClearProviders())).CreateClient();

    [Theory]
    [InlineData("/api/convenios-cobranca")]
    [InlineData("/api/convenios-cobranca/bancos")]
    [InlineData("/api/coberturas")]
    [InlineData("/api/planos")]
    [InlineData("/api/apolices/convenios-cobranca/opcoes")]
    [InlineData("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/modulos/8d198830-f45d-4236-8b37-80cfac9d4b88/plano")]
    [InlineData("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/modulos/8d198830-f45d-4236-8b37-80cfac9d4b88/plano/coberturas/opcoes")]
    public async Task Consultas_SemToken_ExigemAutenticacao(string path)
    {
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/planos/8d198830-f45d-4236-8b37-80cfac9d4b89/coberturas/8d198830-f45d-4236-8b37-80cfac9d4b88")]
    [InlineData("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/coberturas/8d198830-f45d-4236-8b37-80cfac9d4b88/premios")]
    [InlineData("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/modulos/8d198830-f45d-4236-8b37-80cfac9d4b88/plano")]
    [InlineData("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/modulos/8d198830-f45d-4236-8b37-80cfac9d4b88/plano/coberturas/8d198830-f45d-4236-8b37-80cfac9d4b87")]
    public async Task Premios_SemToken_ExigemAutenticacao(string path)
    {
        var response = await _client.PutAsJsonAsync(path, new { premioTitular = 12.34m, premioConjuge = 0m });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task CoberturaModulo_SemToken_ExigeAutenticacao()
    {
        var response = await _client.PostAsJsonAsync("/api/apolices/8d198830-f45d-4236-8b37-80cfac9d4b89/modulos/8d198830-f45d-4236-8b37-80cfac9d4b88/plano/coberturas", new { nome = "Cobertura", premioTitular = 1.25m, premioConjuge = 0m });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
