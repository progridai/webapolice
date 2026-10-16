using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using WebApolice.Api.Infrastructure;

namespace WebApolice.Api.Tests;

public class PadronizacaoTextoMiddlewareTests
{
    [Fact]
    public async Task NormalizaAntesDoBindingEPreservaCamposTecnicosERestauraStream()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.ContentType = "application/json";
        const string body = """{"nome":"JOÃO DA SILVA","email":"ABC@EXAMPLE.COM","codigo":"abc-X","senha":"aBcD","itens":[{"descricao":"AÇÃO ESPECIAL"}]}""";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.Body = stream;
        context.Request.ContentLength = stream.Length;
        var middleware = new PadronizacaoTextoMiddleware(async c =>
        {
            var json = (await JsonNode.ParseAsync(c.Request.Body))!;
            Assert.Equal("João da Silva", (string?)json["nome"]);
            Assert.Equal("Ação Especial", (string?)json["itens"]![0]!["descricao"]);
            Assert.Equal("ABC@EXAMPLE.COM", (string?)json["email"]);
            Assert.Equal("abc-X", (string?)json["codigo"]);
            Assert.Equal("aBcD", (string?)json["senha"]);
        });
        await middleware.InvokeAsync(context);
        Assert.Same(stream, context.Request.Body);
        Assert.Equal(stream.Length, context.Request.ContentLength);
    }

    [Theory]
    [InlineData("application/json", "{invalid", "POST")]
    [InlineData("text/plain", "NOME ORIGINAL", "POST")]
    [InlineData("application/json", "{invalid", "GET")]
    public async Task PreservaRequisicoesForaDoEscopoEJsonInvalido(string contentType, string body, string method)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        var middleware = new PadronizacaoTextoMiddleware(async c =>
            Assert.Equal(body, await new StreamReader(c.Request.Body).ReadToEndAsync()));
        await middleware.InvokeAsync(context);
    }
    [Fact]
    public void TodosOsContextosDeNegocioDevemAplicarPadronizacao()
    {
        var contexts = typeof(Program).Assembly.GetReferencedAssemblies()
            .Where(a => a.Name!.StartsWith("WebApolice.") && !a.Name.Contains("Auditoria"))
            .Select(System.Reflection.Assembly.Load)
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(t));
        Assert.NotEmpty(contexts);
        Assert.All(contexts, type => Assert.True(
            typeof(WebApolice.Shared.Infrastructure.Persistence.TextoPadronizadoDbContext).IsAssignableFrom(type),
            $"{type.FullName} deve herdar de TextoPadronizadoDbContext"));
    }

}
