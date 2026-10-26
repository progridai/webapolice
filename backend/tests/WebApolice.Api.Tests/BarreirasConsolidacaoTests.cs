using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;

namespace WebApolice.Api.Tests;

public class BarreirasConsolidacaoTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    public BarreirasConsolidacaoTests(ApiTestFactory factory) => _factory = factory;

    [Theory]
    [InlineData("erp-ausente")]
    [InlineData("erp-inativo")]
    [InlineData("erp-leitor")]
    public async Task PropostasNegamUsuariosSemPermissaoOuInativos(string sub)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: sub, roles: ["admin"]));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/propostas", new { clienteId = 1 })).StatusCode);
    }

    [Fact]
    public async Task PropostasSemTokenRetornam401()
    {
        using var client = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/propostas")).StatusCode);
    }

    [Fact]
    public async Task PropostaAutorizadaSemStatusDefinidoRejeitaAntesDoBanco()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "erp-total"));
        var response = await client.PostAsJsonAsync("/api/propostas", new { clienteId = 1 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("status inicial", await response.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MediatorDePropostasAtivaHandlerERejeitaPaginacaoInvalida()
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "erp-total"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await client.GetAsync("/api/propostas?pagina=0")).StatusCode);
    }

    [Fact]
    public async Task LimiteDeRequisicoesRetorna429ComTraceId()
    {
        await using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?> { ["RateLimiting:PermitLimit"] = "1" })));
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/version")).StatusCode);
        var response = await client.GetAsync("/api/version");
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.Contains("traceId", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task FalhaDoProvedorDeCepNaoExpoeExcecao()
    {
        var provider = new CepIndisponivel();
        await using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<WebApolice.SharedKernel.Application.Ports.ICEPProvider>();
            services.AddSingleton<WebApolice.SharedKernel.Application.Ports.ICEPProvider>(provider);
        }));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste());
        var response = await client.GetAsync("/api/enderecos/cep/90000000");
        Assert.True(provider.Consultado);
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", body);
        Assert.DoesNotContain("private-cep-test-marker", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    private sealed class CepIndisponivel : WebApolice.SharedKernel.Application.Ports.ICEPProvider
    {
        public bool Consultado { get; private set; }
        public Task<WebApolice.SharedKernel.Application.Models.EnderecoProviderDto?> ConsultarCEPAsync(string cep, CancellationToken cancellationToken)
        {
            Consultado = true;
            return Task.FromException<WebApolice.SharedKernel.Application.Models.EnderecoProviderDto?>(new InvalidOperationException("private-cep-test-marker"));
        }
    }
}
