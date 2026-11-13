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

namespace WebApolice.Api.Tests.Modulos.Clientes;

public class MatriculaNaVidaAuthTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Theory]
    [InlineData("POST", false, false, false, HttpStatusCode.UnprocessableEntity)]
    [InlineData("POST", true, false, false, HttpStatusCode.Forbidden)]
    [InlineData("POST", true, true, false, HttpStatusCode.UnprocessableEntity)]
    [InlineData("PUT", false, false, false, HttpStatusCode.UnprocessableEntity)]
    [InlineData("PUT", true, false, false, HttpStatusCode.Forbidden)]
    [InlineData("PUT", true, true, false, HttpStatusCode.UnprocessableEntity)]
    [InlineData("POST", false, false, true, HttpStatusCode.Forbidden)]
    [InlineData("POST", false, true, true, HttpStatusCode.UnprocessableEntity)]
    [InlineData("PUT", false, false, true, HttpStatusCode.Forbidden)]
    [InlineData("PUT", false, true, true, HttpStatusCode.UnprocessableEntity)]
    public async Task CadastroContextualExigePermissaoAdicionalSemBloquearSelecao(string method, bool nova, bool alterarCliente, bool completar, HttpStatusCode expected)
    {
        await using var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.AddScoped<IPermissoesEfetivasService>(_ => new PermissoesVida(alterarCliente));
            services.AddScoped<IDisponibilidadeCatalogo, CatalogoVida>();
            services.AddScoped<IOperacaoVidas, OperacaoSemBanco>();
        }));
        using var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            ApiTestFactory.GerarTokenDeTeste(sub: Guid.NewGuid().ToString()));
        var apolice = Guid.NewGuid();
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/apolices/{apolice}/vidas" + (method == "PUT" ? $"/{Guid.NewGuid()}" : ""))
        {
            Content = JsonContent.Create(new { clientePublicId = Guid.NewGuid(),
                clienteVinculoPublicId = nova ? (Guid?)null : Guid.NewGuid(), novaMatricula = nova ? "aBc-12" : null, completarVinculoSemEstipulante = completar })
        };
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.UnprocessableEntity)
        {
            var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            Assert.Equal("Operação autorizada sem banco", problem.GetProperty("detail").GetString());
        }
    }

    private sealed class PermissoesVida(bool alterarCliente) : IPermissoesEfetivasService
    {
        public Task<PermissoesEfetivasUsuario> CalcularPermissoesAsync(string sub, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PermissoesEfetivasUsuario(true, true, false, false, [], [],
                alterarCliente ? ["apolices.vidas.inserir", "apolices.vidas.alterar", "clientes.alterar"] : ["apolices.vidas.inserir", "apolices.vidas.alterar"]));
    }

    private sealed class CatalogoVida : IDisponibilidadeCatalogo
    {
        public Task<bool> PermissaoDisponivelAsync(string permissao, CancellationToken ct) =>
            Task.FromResult(permissao is "apolices.vidas.inserir" or "apolices.vidas.alterar" or "clientes.alterar");
        public Task<bool> RecursoDisponivelAsync(string modulo, string recurso, CancellationToken ct) => Task.FromResult(true);
    }

    private sealed class OperacaoSemBanco : IOperacaoVidas
    {
        public Task<T> ExecutarAsync<T>(string acao,
            Func<CancellationToken, Task<(T Resultado, string PublicId, object? Antes, object? Depois)>> executar, CancellationToken ct) =>
            throw new ValidacaoException("Operação autorizada sem banco");
    }
}
