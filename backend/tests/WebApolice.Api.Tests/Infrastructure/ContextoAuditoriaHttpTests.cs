using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using WebApolice.Api.Infrastructure;
using Xunit;

namespace WebApolice.Api.Tests.Infrastructure;

public class ContextoAuditoriaHttpTests
{
    private HttpContext CriarHttpContextMock(string? correlationId = null)
    {
        var context = new DefaultHttpContext();
        if (correlationId != null)
        {
            context.Request.Headers["X-Correlation-ID"] = new StringValues(correlationId);
        }
        return context;
    }

    private IHttpContextAccessor CriarAccessorMock(HttpContext context)
    {
        return new HttpContextAccessor { HttpContext = context };
    }

    [Fact]
    public void ObterCorrelationId_ShouldReturnNull_WhenHeaderIsMissing()
    {
        var accessor = CriarAccessorMock(CriarHttpContextMock());
        var sut = new ContextoAuditoriaHttp(accessor);

        var result = sut.ObterCorrelationId();

        Assert.Null(result);
    }

    [Fact]
    public void ObterCorrelationId_ShouldReturnString_WhenHeaderIsValid()
    {
        var accessor = CriarAccessorMock(CriarHttpContextMock("abc-123"));
        var sut = new ContextoAuditoriaHttp(accessor);

        var result = sut.ObterCorrelationId();

        Assert.Equal("abc-123", result);
    }

    [Fact]
    public void ObterCorrelationId_ShouldRemoveNewlines_ToPreventHeaderInjection()
    {
        var accessor = CriarAccessorMock(CriarHttpContextMock("abc\r\n-123\n"));
        var sut = new ContextoAuditoriaHttp(accessor);

        var result = sut.ObterCorrelationId();

        Assert.Equal("abc-123", result);
    }

    [Fact]
    public void ObterCorrelationId_ShouldTruncateTo255Characters()
    {
        var longString = new string('A', 300);
        var accessor = CriarAccessorMock(CriarHttpContextMock(longString));
        var sut = new ContextoAuditoriaHttp(accessor);

        var result = sut.ObterCorrelationId();

        Assert.Equal(255, result?.Length);
        Assert.Equal(new string('A', 255), result);
    }
}
