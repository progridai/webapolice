using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Me;
using WebApolice.Modulos.Seguranca.Domain;

namespace WebApolice.Api.Tests;

public sealed class AtalhosHomeHttpTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private const string Endpoint = "/api/seguranca/me/atalhos-home";

    [Fact]
    public async Task ExigeAutenticacaoEGravaSomenteContaAtualSemPermissaoAdministrativa()
    {
        var repository = new Repository();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IPreferenciasHomeRepository>(_ => repository)));
        using var client = host.CreateClient();
        using (var response = await client.GetAsync(Endpoint)) Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using (var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = new[] { "/clientes" } })) Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, repository.Gravacoes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "conta-a", roles: []));
        var inicial = await client.GetFromJsonAsync<PreferenciasHomeDto>(Endpoint);
        Assert.Null(inicial!.AtalhosHome);
        using (var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = new[] { "/clientes", "/seguranca/usuarios" }, usuarioId = "conta-b" }))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var salvo = await response.Content.ReadFromJsonAsync<PreferenciasHomeDto>();
            Assert.Equal(["/clientes", "/seguranca/usuarios"], Assert.IsType<string[]>(salvo!.AtalhosHome));
        }
        Assert.Null(repository.Usuarios["conta-b"].AtalhosHome);
        Assert.Equal("conta-a", repository.UltimoSub);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "conta-b"));
        using (var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = Array.Empty<string>() })) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<PreferenciasHomeDto>(Endpoint))!.AtalhosHome!);
        Assert.Equal(["/clientes", "/seguranca/usuarios"], Assert.IsType<string[]>(repository.Usuarios["conta-a"].AtalhosHome));
    }

    [Theory]
    [InlineData("inativo")]
    [InlineData("ausente")]
    public async Task UsuarioLocalInativoOuAusenteRecebe403(string sub)
    {
        var repository = new Repository();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IPreferenciasHomeRepository>(_ => repository)));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: sub));
        using (var response = await client.GetAsync(Endpoint)) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using (var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = new[] { "/clientes" } })) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, repository.Gravacoes);
    }

    [Theory]
    [InlineData("/app")]
    [InlineData("/CLIENTES")]
    [InlineData("https://exemplo.test")]
    [InlineData("/clientes?usuario=outro")]
    public async Task DestinoInvalidoNaoGrava(string destino)
    {
        var repository = new Repository();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IPreferenciasHomeRepository>(_ => repository)));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "conta-a"));
        using var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = new[] { destino } });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(0, repository.Gravacoes);
    }

    [Fact]
    public async Task SelecaoNullOuAusenteRecebe400()
    {
        var repository = new Repository();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IPreferenciasHomeRepository>(_ => repository)));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: "conta-a"));
        using (var response = await client.PutAsJsonAsync(Endpoint, new { atalhosHome = (string[]?)null })) Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using (var response = await client.PutAsJsonAsync(Endpoint, new { })) Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, repository.Gravacoes);
    }

    private sealed class Repository : IPreferenciasHomeRepository
    {
        public Dictionary<string, Usuario> Usuarios { get; } = new()
        {
            ["conta-a"] = Usuario.Criar("conta-a", "usuario.a", "Maria Silva", "maria@local.test"),
            ["conta-b"] = Usuario.Criar("conta-b", "usuario.b", "João Silva", "joao@local.test"),
            ["inativo"] = new Usuario("inativo", "inativo", "Inativo", "inativo@local.test", false)
        };
        public string? UltimoSub { get; private set; }
        public int Gravacoes { get; private set; }
        public Task<Usuario?> ObterUsuarioAtivoAsync(string keycloakSub, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UltimoSub = keycloakSub;
            return Task.FromResult(Usuarios.TryGetValue(keycloakSub, out var usuario) && usuario.Ativo ? usuario : null);
        }
        public Task SalvarAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Gravacoes++;
            return Task.CompletedTask;
        }
    }
}
