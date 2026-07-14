using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace WebApolice.Api.Tests.Modulos.Clientes;

public class ClientesControllerAuthTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public ClientesControllerAuthTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("GET", "/api/clientes")]
    [InlineData("GET", "/api/clientes/1")]
    [InlineData("POST", "/api/clientes")]
    [InlineData("PUT", "/api/clientes/1")]
    [InlineData("POST", "/api/clientes/1/ativar")]
    [InlineData("POST", "/api/clientes/1/inativar")]
    public async Task Endpoints_SemAutenticacao_DevemRetornar401(string method, string url)
    {
        var message = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST" || method == "PUT") message.Content = JsonContent.Create(new { });
        var response = await _client.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("POST", "/api/clientes")]
    [InlineData("PUT", "/api/clientes/1")]
    [InlineData("POST", "/api/clientes/1/ativar")]
    [InlineData("POST", "/api/clientes/1/inativar")]
    public async Task AcoesDeGestor_ComRoleOperador_DevemRetornar403(string method, string url)
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: new[] { "operador" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var message = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST" || method == "PUT") message.Content = JsonContent.Create(new { });
        var response = await _client.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cadastro_ComRoleAdmin_NaoDeveRetornar403()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: new[] { "admin" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var request = new { TipoPessoa = 1, Nome = "Teste", Documento = "00000000000" };
        var response = await _client.PostAsJsonAsync("/api/clientes", request);
        
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Consulta_ComRoleOperador_NaoDeveRetornar403()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: new[] { "operador" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/clientes/999");
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Delete_MetodoProibido_DeveRetornar405Ou404()
    {
        var token = ApiTestFactory.GerarTokenDeTeste(roles: new[] { "admin" });
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync("/api/clientes/1");
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }
}
