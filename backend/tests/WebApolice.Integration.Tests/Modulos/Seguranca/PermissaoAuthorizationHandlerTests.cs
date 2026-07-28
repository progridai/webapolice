using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApolice.Integration.Tests.Setup;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class PermissaoAuthorizationHandlerTests : IClassFixture<SegurancaIntegrationTestFixture>
{
    private readonly Mock<IContextoUsuarioAutenticado> _contextoMock;
    private readonly Mock<IPermissoesEfetivasService> _permissoesServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly PermissaoAuthorizationHandler _handler;

    public PermissaoAuthorizationHandlerTests(SegurancaIntegrationTestFixture fixture)
    {
        _contextoMock = new Mock<IContextoUsuarioAutenticado>();
        _permissoesServiceMock = new Mock<IPermissoesEfetivasService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        // Utiliza o DbContext com PostgreSQL real, evitando incompatibilidade do InMemory provider
        var dbContext = fixture.DbContext;

        _handler = new PermissaoAuthorizationHandler(
            _contextoMock.Object,
            _permissoesServiceMock.Object,
            _httpContextAccessorMock.Object,
            dbContext);
    }

    [Fact]
    public async Task Nao_Autenticado_Nao_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(false);
        var requirement = new PermissaoRequirement("clientes.visualizar");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Sem_KeycloakSub_Nao_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns(string.Empty);
        
        var requirement = new PermissaoRequirement("clientes.visualizar");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Usuario_Nao_Encontrado_Nao_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _permissoesServiceMock.Setup(p => p.CalcularPermissoesAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(false, false, false, false, new HashSet<string>(), new HashSet<string>()));
        
        var requirement = new PermissaoRequirement("clientes.visualizar");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Usuario_Inativo_Nao_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _permissoesServiceMock.Setup(p => p.CalcularPermissoesAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(true, false, false, false, new HashSet<string> { "CLIENTES" }, new HashSet<string> { "clientes.visualizar" }));
        
        var requirement = new PermissaoRequirement("clientes.visualizar");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Sem_Permissao_Exata_Nao_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _permissoesServiceMock.Setup(p => p.CalcularPermissoesAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(true, true, false, false, new HashSet<string> { "CLIENTES" }, new HashSet<string> { "clientes.alterar" }));
        
        var requirement = new PermissaoRequirement("clientes.visualizar"); // Similar mas diferente
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Com_Permissao_Exata_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _permissoesServiceMock.Setup(p => p.CalcularPermissoesAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(true, true, false, false, new HashSet<string> { "CLIENTES" }, new HashSet<string> { "clientes.visualizar" }));
        
        var requirement = new PermissaoRequirement("clientes.visualizar");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Com_AcessoTotal_Recebe_Autorizacao()
    {
        _contextoMock.Setup(c => c.EstaAutenticado).Returns(true);
        _contextoMock.Setup(c => c.KeycloakSub).Returns("123");

        _permissoesServiceMock.Setup(p => p.CalcularPermissoesAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PermissoesEfetivasUsuario(true, true, true, false, new HashSet<string>(), new HashSet<string>()));
        
        var requirement = new PermissaoRequirement("qualquer.permissao");
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);

        await _handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
}
