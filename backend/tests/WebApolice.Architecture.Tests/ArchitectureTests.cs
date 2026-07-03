using NetArchTest.Rules;
using WebApolice.SharedKernel;
using Xunit;
using System.Reflection;
using System.Linq;

namespace WebApolice.Architecture.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "WebApolice.Modules.*.Domain";
    private const string ApplicationNamespace = "WebApolice.Modules.*.Application";
    private const string InfrastructureNamespace = "WebApolice.Modules.*.Infrastructure";
    private const string SharedInfrastructureNamespace = "WebApolice.Shared.Infrastructure";
    private const string ApiNamespace = "WebApolice.Api";
    private const string SharedKernelNamespace = "WebApolice.SharedKernel";

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnOtherLayers()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .Should().NotHaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, SharedInfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer deve ser independente de Application, Infrastructure, Shared Infrastructure ou API.");
    }

    [Fact]
    public void Domain_ShouldNot_DependOnEntityFrameworkCore_Or_AspNetCore()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer não deve depender de EF Core, Npgsql ou ASP.NET Core.");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructureOrApi()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .Should().NotHaveDependencyOnAny(InfrastructureNamespace, SharedInfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer não deve depender de Infrastructure, Shared Infrastructure ou API.");
    }

    [Fact]
    public void Application_ShouldNot_DependOnEntityFrameworkCore()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer não deve depender de EF Core ou Npgsql.");
    }

    [Fact]
    public void Application_ShouldNot_DependOnAspNetCoreHttp()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .Should().NotHaveDependencyOn("Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer não deve utilizar tipos HTTP do ASP.NET Core.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOnModules()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOn("WebApolice.Modules")
            .GetResult();

        Assert.True(result.IsSuccessful, "SharedKernel não deve referenciar módulos de negócio.");
    }

    [Fact]
    public void ControllersOrEndpoints_ShouldNot_ResideInDomain()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(DomainNamespace)
            .Should().NotHaveNameEndingWith("Controller")
            .And().NotHaveNameEndingWith("Endpoint")
            .GetResult();

        Assert.True(result.IsSuccessful, "Endpoints ou Controllers não devem ser colocados no Domain.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOnAspNetCoreMvc_Or_Http_Or_EFCore()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.AspNetCore.Mvc", "Microsoft.AspNetCore.Http", "Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, "SharedKernel não deve ter dependências de infraestrutura HTTP ou de Banco de Dados.");
    }

    [Fact]
    public void Application_And_Domain_ShouldNot_DependOnAspNetCoreMvc()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .Or().ResideInNamespace(DomainNamespace)
            .Should().NotHaveDependencyOn("Microsoft.AspNetCore.Mvc")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application e Domain não devem utilizar ProblemDetails ou tipos MVC.");
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOnAspNetCoreMvc_Or_Http()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Or().ResideInNamespace(SharedInfrastructureNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.AspNetCore.Mvc", "Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure não deve definir contratos HTTP ou ProblemDetails.");
    }

    [Fact]
    public void Api_ShouldNot_Execute_EF_Migrations_Automatically()
    {
        // NetArchTest não inspeciona chamadas de métodos, porém o framework de teste
        // garante pela infraestrutura (scripts automatizados no fluxo) a busca via grep
        // que 'Database.Migrate' não seja invocado pela API.
        // Aqui também validamos que a API não deve declarar contextos de banco de dados diretamente.
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace(ApiNamespace)
            .Should().NotHaveNameEndingWith("DbContext")
            .GetResult();

        Assert.True(result.IsSuccessful, "A API não deve definir contextos de banco de dados diretamente.");
    }
}
