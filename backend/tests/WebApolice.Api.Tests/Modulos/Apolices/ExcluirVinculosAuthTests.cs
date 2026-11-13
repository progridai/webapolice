using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;

namespace WebApolice.Api.Tests.Modulos.Apolices;

public sealed class ExcluirVinculosAuthTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    public static TheoryData<string, TipoVinculoApolice, string> Rotas => new()
    {
        { "ramos", TipoVinculoApolice.Ramo, "apolices.ramos.excluir" },
        { "subgrupos", TipoVinculoApolice.Subgrupo, "apolices.subgrupos.excluir" },
        { "subestipulantes", TipoVinculoApolice.Subestipulante, "apolices.subestipulantes.excluir" },
        { "modulos", TipoVinculoApolice.Modulo, "apolices.modulos.excluir" },
        { "planos-modulo", TipoVinculoApolice.PlanoModulo, "apolices.modulos.excluir" },
        { "coberturas-modulo", TipoVinculoApolice.CoberturaModulo, "apolices.modulos.excluir" },
        { "produtos-anteriores", TipoVinculoApolice.ProdutoAnterior, "apolices.universo.excluir" },
        { "planos-anteriores", TipoVinculoApolice.PlanoAnterior, "apolices.universo.excluir" },
        { "coberturas-anteriores", TipoVinculoApolice.CoberturaAnterior, "apolices.universo.excluir" }
    };
    [Theory, MemberData(nameof(Rotas))]
    public async Task ExigeAutenticacaoPermissaoEspecificaECatalogo(string rota, TipoVinculoApolice tipo, string codigo)
    {
        var permissoes = new Permissoes { Codigo = codigo.Replace("excluir", "inativar") };
        var catalogo = new Catalogo();
        var exclusao = new ExclusaoStub();
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddScoped<IPermissoesEfetivasService>(_ => permissoes);
            services.AddScoped<IDisponibilidadeCatalogo>(_ => catalogo);
            services.AddScoped<IExclusaoVinculosApolice>(_ => exclusao);
        }));
        using var client = host.CreateClient();
        var apolice = Guid.NewGuid(); var vinculo = Guid.NewGuid(); var usuario = Guid.NewGuid();
        var url = $"/api/apolices/{apolice}/vinculos/{rota}/{vinculo}";
        using (var response = await client.DeleteAsync(url)) Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiTestFactory.GerarTokenDeTeste(sub: usuario.ToString()));
        using (var response = await client.DeleteAsync(url)) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        permissoes.Codigo = codigo; catalogo.Disponivel = false;
        using (var response = await client.DeleteAsync(url)) Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, exclusao.Chamadas);
        catalogo.Disponivel = true;
        using (var response = await client.DeleteAsync(url)) Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, exclusao.Chamadas);
        Assert.Equal((apolice, tipo, vinculo, usuario), exclusao.Ultima);
    }
    private sealed class ExclusaoStub : IExclusaoVinculosApolice
    {
        public int Chamadas { get; private set; }
        public (Guid, TipoVinculoApolice, Guid, Guid) Ultima { get; private set; }
        public Task ExcluirAsync(Guid apolice, TipoVinculoApolice tipo, Guid vinculo, Guid usuario, CancellationToken ct)
        { Chamadas++; Ultima = (apolice, tipo, vinculo, usuario); return Task.CompletedTask; }
    }
    private sealed class Permissoes : IPermissoesEfetivasService
    {
        public required string Codigo { get; set; }
        public Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string sub, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PermissoesEfetivasUsuario(true, true, false, false, [], [], [Codigo]));
    }
    private sealed class Catalogo : IDisponibilidadeCatalogo
    {
        public bool Disponivel { get; set; } = true;
        public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken ct) => Task.FromResult(Disponivel);
        public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken ct) => Task.FromResult(true);
    }
}
