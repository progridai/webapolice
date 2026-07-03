using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebApolice.Api.Tests;

public class ExceptionHandlingTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _client;

    public ExceptionHandlingTests(ApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_RecursoNaoEncontrado_DeveRetornar404ComProblemDetailsPadronizado()
    {
        var response = await _client.GetAsync("/api/test/not-found");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/recurso-nao-encontrado", problemDetails.Type);
        Assert.Equal("Recurso não encontrado", problemDetails.Title);
        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Recurso não encontrado no ambiente de testes.", problemDetails.Detail);
        Assert.Equal("/api/test/not-found", problemDetails.Instance);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Get_ConflitoDeNegocio_DeveRetornar409ComProblemDetailsPadronizado()
    {
        var response = await _client.GetAsync("/api/test/conflict");
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/conflito", problemDetails.Type);
        Assert.Equal(409, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Get_RegraDeNegocio_DeveRetornar422ComProblemDetailsPadronizado()
    {
        var response = await _client.GetAsync("/api/test/unprocessable");
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/regra-de-negocio", problemDetails.Type);
        Assert.Equal(422, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Get_ErroInterno_DeveRetornar500ComProblemDetailsSemStackTrace()
    {
        var response = await _client.GetAsync("/api/test/internal-error");
        
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/erro-interno", problemDetails.Type);
        Assert.Equal(500, problemDetails.Status);
        
        // Em testes o ambiente é "Testing", não Development. A stack trace e a mensagem real não devem vazar.
        Assert.Equal("Consulte os logs para obter mais informações.", problemDetails.Detail);
        Assert.DoesNotContain("StackTrace", content);
        Assert.DoesNotContain("Erro interno simulado", content);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Post_RequisicaoInvalida_DeveRetornar400ComValidationProblemDetails()
    {
        var response = await _client.PostAsJsonAsync("/api/test/bad-request", new { Nome = "" });
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/requisicao-invalida", problemDetails.Type);
        Assert.Equal(400, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.NotEmpty(problemDetails.Errors);
        Assert.True(problemDetails.Errors.ContainsKey("Nome"));
    }
    
    [Fact]
    public async Task Post_JsonMalformado_DeveRetornar400()
    {
        var stringContent = new StringContent("{ nome: invalid }", System.Text.Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/test/bad-request", stringContent);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
    }
    
    [Fact]
    public async Task Get_EndpointRequerAutenticacao_DeveRetornar401ComProblemDetailsTraceId()
    {
        // /api/auth/me requer autenticação válida
        var response = await _client.GetAsync("/api/auth/me");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal("https://webapolice/errors/nao-autenticado", problemDetails.Type);
        Assert.Equal(401, problemDetails.Status);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.Equal("É necessário apresentar um token de acesso válido.", problemDetails.Detail); // Mensagem genérica segura
    }

    [Fact]
    public async Task Get_EndpointCancelado_NaoDeveRetornar500()
    {
        var response = await _client.GetAsync("/api/test/canceled");
        
        // Como o middleware captura o OperationCanceledException e retorna false,
        // o framework ASP.NET Core normalmente não escreve um body e apenas aborta ou retorna 200/empty (ou o TestServer trata diferente).
        // A principal verificação é que NÃO deve retornar 500.
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
