using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using WebApolice.Api.Infrastructure.Errors;

namespace WebApolice.Api.Tests;

public class ErroInternoSeguroTests
{
    [Fact]
    public async Task HandlerNaoExpoeDetalhesInternosMesmoSemHostDeTeste()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "trace-seguro" };
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);
        await handler.TryHandleAsync(context, new Exception("internal-private-marker", new Exception("nested-marker")), default);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Contains("trace-seguro", body);
        Assert.DoesNotContain("internal-private-marker", body);
        Assert.DoesNotContain("nested-marker", body);
        Assert.DoesNotContain("System.Exception", body);
    }
}
