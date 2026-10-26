using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace WebApolice.Api.Tests;

public class ErroEmProducaoTests
{
    [Fact]
    public async Task HostProductionNaoExpoeExcecaoInterna()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("Authentication:Authority", "https://idp.example/realms/test");
            builder.UseSetting("Authentication:Audience", "webapolice-api");
            builder.UseSetting("Authentication:RequireHttpsMetadata", "true");
            builder.UseSetting("ConnectionStrings:PostgreSql", "Host=127.0.0.1;Database=unreachable_test;Username=test");
            builder.ConfigureServices(services =>
            {
                services.AddControllers().AddApplicationPart(typeof(WebApolice.Api.Infrastructure.Errors.TestController).Assembly);
                services.AddScoped<WebApolice.Modulos.Seguranca.Application.Ports.IProvisionamentoUsuarioService, ProvisionamentoIsolado>();
            });
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        Assert.Equal("Production", factory.Services.GetRequiredService<IWebHostEnvironment>().EnvironmentName);
        var response = await client.GetAsync("/api/test/internal-error-controller");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", body);
        Assert.DoesNotContain("private-production-test-marker", body);
        Assert.DoesNotContain("System.Exception", body);
        Assert.DoesNotContain("StackTrace", body);
    }
}
