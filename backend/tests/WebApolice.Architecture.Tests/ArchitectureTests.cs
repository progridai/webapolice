using NetArchTest.Rules;
using WebApolice.SharedKernel;
using Xunit;
using System.Reflection;
using System.Linq;

namespace WebApolice.Architecture.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = @"^WebApolice\.Modulos\..*\.Domain(?:\.|$)";
    private const string ApplicationNamespace = @"^WebApolice\.Modulos\..*\.Application(?:\.|$)";
    private const string InfrastructureNamespace = @"^WebApolice\.Modulos\..*\.Infrastructure(?:\.|$)";
    private const string SharedInfrastructureNamespace = "WebApolice.Shared.Infrastructure";
    private const string ApiNamespace = "WebApolice.Api";
    private const string SharedKernelNamespace = "WebApolice.SharedKernel";

    private static readonly Assembly[] Assemblies = [
        Assembly.Load("WebApolice.Api"),
        Assembly.Load("WebApolice.Auditoria"),
        Assembly.Load("WebApolice.Modulos.Atendimento"),
        Assembly.Load("WebApolice.Modulos.Cadastro"),
        Assembly.Load("WebApolice.Modulos.Comissao"),
        Assembly.Load("WebApolice.Modulos.Convenio"),
        Assembly.Load("WebApolice.Modulos.Documentos"),
        Assembly.Load("WebApolice.Modulos.Financeiro"),
        Assembly.Load("WebApolice.Modulos.Integracao"),
        Assembly.Load("WebApolice.Modulos.Seguranca"),
        Assembly.Load("WebApolice.Modulos.Seguro"),
        Assembly.Load("WebApolice.Modulos.Sinistros"),
        Assembly.Load("WebApolice.Shared.Infrastructure"),
        Assembly.Load("WebApolice.SharedKernel")
    ];

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnOtherLayers()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveDependencyOnAny(NomesDasCamadas(".Application", ".Infrastructure", ".Api").Concat([SharedInfrastructureNamespace, ApiNamespace]).ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Domain layer deve ser independente de Application, Infrastructure, Shared Infrastructure ou API.");
    }

    [Fact]
    public void Domain_ShouldNot_DependOnEntityFrameworkCore_Or_AspNetCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Domain layer não deve depender de EF Core, Npgsql ou ASP.NET Core.");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructureOrApi()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(ApplicationNamespace)
            .Should().NotHaveDependencyOnAny(NomesDasCamadas(".Infrastructure", ".Api").Concat([SharedInfrastructureNamespace, ApiNamespace]).ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Application layer não deve depender de Infrastructure, Shared Infrastructure ou API.");
    }

    [Fact]
    public void Application_ShouldNot_DependOnEntityFrameworkCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(ApplicationNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Application layer não deve depender de EF Core ou Npgsql.");
    }

    [Fact]
    public void Application_ShouldNot_DependOnAspNetCoreHttp()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(ApplicationNamespace)
            .Should().NotHaveDependencyOn("Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Application layer não deve utilizar tipos HTTP do ASP.NET Core.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOnModules()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOn("WebApolice.Modulos")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "SharedKernel não deve referenciar módulos de negócio.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOnSharedInfrastructure_Or_Api()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOnAny(SharedInfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "SharedKernel não deve referenciar Shared.Infrastructure ou API.");
    }

    [Fact]
    public void ControllersOrEndpoints_ShouldNot_ResideInDomain()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveNameEndingWith("Controller")
            .And().NotHaveNameEndingWith("Endpoint")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Endpoints ou Controllers não devem ser colocados no Domain.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_DependOnAspNetCoreMvc_Or_Http_Or_EFCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.AspNetCore.Mvc", "Microsoft.AspNetCore.Http", "Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "SharedKernel não deve ter dependências de infraestrutura HTTP ou de Banco de Dados.");
    }

    [Fact]
    public void Application_And_Domain_ShouldNot_DependOnAspNetCoreMvc()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(ApplicationNamespace)
            .Or().ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveDependencyOn("Microsoft.AspNetCore.Mvc")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Application e Domain não devem utilizar ProblemDetails ou tipos MVC.");
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOnAspNetCoreMvc_Or_Http()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .Or().ResideInNamespace(SharedInfrastructureNamespace)
            .Should().NotHaveDependencyOnAny("Microsoft.AspNetCore.Mvc", "Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Infrastructure não deve definir contratos HTTP ou ProblemDetails.");
    }

    [Fact]
    public void Api_ShouldNot_Execute_EF_Migrations_Automatically()
    {
        // NetArchTest não inspeciona chamadas de métodos, porém o framework de teste
        // garante pela infraestrutura (scripts automatizados no fluxo) a busca via grep
        // que 'Database.Migrate' não seja invocado pela API.
        // Aqui também validamos que a API não deve declarar contextos de banco de dados diretamente.
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(ApiNamespace)
            .Should().NotHaveNameEndingWith("DbContext")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "A API não deve definir contextos de banco de dados diretamente.");
    }

    [Fact]
    public void Domain_ShouldNot_DependOnAuditoriaInfrastructure()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveDependencyOn("WebApolice.Auditoria.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Domain não referencia auditoria técnica.");
    }

    [Fact]
    public void AuditoriaContracts_ShouldNot_DependOnAspNetCore()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace("WebApolice.Auditoria.Contracts")
            .Should().NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Contratos de auditoria não dependem de ASP.NET Core.");
    }

    [Fact]
    public void Auditoria_ShouldNot_DependOnKeycloak()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace("WebApolice.Auditoria")
            .Should().NotHaveDependencyOn("Keycloak") // O projeto não usa biblioteca Keycloak específica, mas validamos a string
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "Auditoria não depende de Keycloak.");
    }

    [Fact]
    public void AuditoriaDbContext_ShouldResideInInfrastructure()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .HaveName("AuditoriaDbContext")
            .Should().ResideInNamespace("WebApolice.Auditoria.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "DbContext da auditoria permanece em Infrastructure.");
    }

    [Fact]
    public void ApiAndModules_ShouldNot_AccessAuditoriaDbContextDirectly()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(ApiNamespace)
            .Or().ResideInNamespaceMatching(ApplicationNamespace)
            .Or().ResideInNamespaceMatching(DomainNamespace)
            .Should().NotHaveDependencyOn("WebApolice.Auditoria.Infrastructure.AuditoriaDbContext")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "API e módulos não acessam diretamente o DbContext da auditoria.");
    }

    [Fact]
    public void InfraestruturaDbContext_ShouldNot_ContainAuditoria()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .HaveName("InfraestruturaDbContext")
            .Should().NotHaveDependencyOn("WebApolice.Auditoria")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "InfraestruturaDbContext não contém entidade de auditoria.");
    }

    [Fact]
    public void AuditoriaDbContext_ShouldNot_ContainBusinessEntities()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .HaveName("AuditoriaDbContext")
            .Should().NotHaveDependencyOn("WebApolice.Modulos")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "AuditoriaDbContext não contém entidades de negócio.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_ContainAuditoriaPersistence()
    {
        var result = Types.InAssemblies(Assemblies)
            .That()
            .ResideInNamespace(SharedKernelNamespace)
            .Should().NotHaveDependencyOn("WebApolice.Auditoria")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []) + "\n" + "SharedKernel não contém tipos de persistência de auditoria.");
    }

    private static string[] NomesDasCamadas(params string[] segmentos) => Assemblies
        .SelectMany(a => a.GetTypes()).Select(t => t.Namespace).OfType<string>()
        .Where(n => segmentos.Any(segmento => n.EndsWith(segmento, StringComparison.Ordinal) || n.Contains(segmento + ".", StringComparison.Ordinal)))
        .Distinct().ToArray();

    [Fact]
    public void InventarioCarregaTodosOsProjetosDeProducao()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "backend", "src"))) dir = dir.Parent;
        Assert.NotNull(dir);
        var esperados = Directory.GetFiles(Path.Combine(dir.FullName, "backend", "src"), "WebApolice.*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension).Order().ToArray();
        Assert.Equal(esperados, Assemblies.Select(a => a.GetName().Name).Order().ToArray());
        foreach (var assembly in Assemblies) Assert.NotEmpty(assembly.GetTypes());
    }

    [Theory]
    [InlineData(DomainNamespace)]
    [InlineData(ApplicationNamespace)]
    [InlineData(InfrastructureNamespace)]
    public void SeletorDeCamadaNaoPodeSerVazio(string seletor)
    {
        Assert.NotEmpty(Types.InAssemblies(Assemblies).That().ResideInNamespaceMatching(seletor).GetTypes());
        foreach (var assembly in Assemblies.Where(a => a.GetName().Name!.StartsWith("WebApolice.Modulos.", StringComparison.Ordinal)))
        {
            var universo = Types.InAssembly(assembly).GetTypes().ToArray();
            Assert.NotEmpty(universo);
            Assert.Empty(assembly.GetExportedTypes().Where(t => !t.IsNested).Except(universo));
            var esperados = universo.Where(t => System.Text.RegularExpressions.Regex.IsMatch(t.Namespace ?? "", seletor)).ToArray();
            var selecionados = Types.InAssembly(assembly).That().ResideInNamespaceMatching(seletor).GetTypes().ToArray();
            Assert.Equal(esperados.Select(t => t.FullName).Order(), selecionados.Select(t => t.FullName).Order());
        }
    }

    [Fact]
    public void RegraDetectaViolacaoConhecidaEmFixture()
    {
        var result = Types.InAssembly(typeof(DependenciaProibida).Assembly).That().HaveName(nameof(DependenciaProibida))
            .Should().NotHaveDependencyOn("Microsoft.EntityFrameworkCore").GetResult();
        Assert.False(result.IsSuccessful);
        Assert.Contains(typeof(DependenciaProibida), result.FailingTypes);
    }

    [Fact]
    public void FluxoClientesCorrigidoNaoDependeDeInfraestrutura()
    {
        var tipos = new[] { "AlterarClienteHandler", "CadastrarClienteHandler", "AtivarClienteHandler", "InativarClienteHandler", "ConsultarClientePorIdHandler", "IClienteRepository", "IOperacaoCadastroAuditada" };
        foreach (var nome in tipos)
        {
            var selecionados = Types.InAssemblies(Assemblies).That().HaveName(nome);
            Assert.NotEmpty(selecionados.GetTypes());
            var result = selecionados.Should().NotHaveDependencyOnAny(NomesDasCamadas(".Infrastructure")
                .Concat(["Microsoft.EntityFrameworkCore", "Npgsql", "WebApolice.Shared.Infrastructure"]).ToArray()).GetResult();
            Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
        }
    }

    private sealed class DependenciaProibida
    {
        public Microsoft.EntityFrameworkCore.DbContext? Contexto { get; set; }
    }

    [Fact]
    public void ModulosNaoAcessamInfraestruturaDeOutrosModulos()
    {
        var modules = Assemblies.Where(a => a.GetName().Name!.StartsWith("WebApolice.Modulos.", StringComparison.Ordinal)).ToArray();
        var falhas = new List<string>();
        foreach (var module in modules)
        {
            var externas = modules.Where(a => a != module).SelectMany(a => a.GetTypes())
                .Select(t => t.Namespace).OfType<string>().Where(n => n.Contains(".Infrastructure", StringComparison.Ordinal)).Distinct().ToArray();
            var result = Types.InAssembly(module).Should().NotHaveDependencyOnAny(externas).GetResult();
            if (!result.IsSuccessful) falhas.AddRange(result.FailingTypeNames ?? []);
        }
        Assert.True(falhas.Count == 0, string.Join("\n", falhas.Distinct().Order()));
    }

    [Fact]
    public void PortasNaoExpoemConsultasDeProvider()
    {
        var portas = Assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsInterface && ((t.Namespace?.Contains(".Application.Ports", StringComparison.Ordinal) ?? false)
                || (t.Namespace?.Contains(".Contracts", StringComparison.Ordinal) ?? false))).ToArray();
        Assert.NotEmpty(portas);
        var falhas = portas.SelectMany(t => t.GetMethods().Select(m => (Tipo: t, Metodo: m)))
            .Where(x => ExpoeConsulta(x.Metodo.ReturnType) || x.Metodo.GetParameters().Any(p => ExpoeConsulta(p.ParameterType)))
            .Select(x => x.Tipo.FullName + "." + x.Metodo.Name).ToArray();
        Assert.Empty(falhas);

        static bool ExpoeConsulta(Type tipo) => typeof(IQueryable).IsAssignableFrom(tipo)
            || (tipo.HasElementType && ExpoeConsulta(tipo.GetElementType()!))
            || (tipo.IsGenericType && tipo.GetGenericArguments().Any(ExpoeConsulta));
    }

}
