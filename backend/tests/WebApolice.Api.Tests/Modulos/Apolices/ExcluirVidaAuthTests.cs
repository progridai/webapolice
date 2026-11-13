using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Api.Tests.Modulos.Apolices;

public class ExcluirVidaAuthTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Theory]
    [InlineData(false, true, "apolices.vidas.excluir", HttpStatusCode.Unauthorized)]
    [InlineData(true, true, "apolices.vidas.inativar", HttpStatusCode.Forbidden)]
    [InlineData(true, true, "apolices.vidas.alterar", HttpStatusCode.Forbidden)]
    [InlineData(true, false, "apolices.vidas.excluir", HttpStatusCode.Forbidden)]
    [InlineData(true, true, "apolices.vidas.excluir", HttpStatusCode.UnprocessableEntity)]
    public async Task DeleteExigePermissaoPropriaECatalogoDisponivel(bool autenticado, bool catalogo, string permissao, HttpStatusCode esperado)
    {
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddScoped<IPermissoesEfetivasService>(_ => new Permissoes(permissao));
            services.AddScoped<IDisponibilidadeCatalogo>(_ => new Catalogo(catalogo));
            services.AddScoped<IOperacaoVidas, OperacaoSemBanco>();
        }));
        using var client = host.CreateClient();
        if (autenticado) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            ApiTestFactory.GerarTokenDeTeste(sub: Guid.NewGuid().ToString()));
        using var response = await client.DeleteAsync($"/api/apolices/{Guid.NewGuid()}/vidas/{Guid.NewGuid()}");
        Assert.Equal(esperado, response.StatusCode);
        if (esperado == HttpStatusCode.UnprocessableEntity)
            Assert.Equal("Exclusão autorizada sem banco", (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()).GetProperty("detail").GetString());
    }

    private sealed class Permissoes(string codigo) : IPermissoesEfetivasService
    {
        public Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string sub, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PermissoesEfetivasUsuario(true, true, false, false, [], [], [codigo]));
    }
    private sealed class Catalogo(bool disponivel) : IDisponibilidadeCatalogo
    {
        public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken ct) => Task.FromResult(disponivel);
        public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken ct) => Task.FromResult(true);
    }
    private sealed class OperacaoSemBanco : IOperacaoVidas
    {
        public Task<T> ExecutarAsync<T>(string acao,
            Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar, CancellationToken ct) =>
            throw new ValidacaoException("Exclusão autorizada sem banco");
    }
}
