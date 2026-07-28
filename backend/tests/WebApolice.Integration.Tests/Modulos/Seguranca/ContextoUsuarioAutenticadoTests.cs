using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApolice.Modulos.Seguranca.Infrastructure.Authentication;
using Xunit;
using FluentAssertions;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class ContextoUsuarioAutenticadoTests
{
    [Fact]
    public void Sem_HttpContext_Deve_Retornar_Nao_Autenticado()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeFalse();
        contexto.KeycloakSub.Should().BeNull();
    }

    [Fact]
    public void Identidade_Nao_Autenticada_Deve_Retornar_Nao_Autenticado()
    {
        var context = new DefaultHttpContext();
        // ClaimsPrincipal sem ClaimsIdentity explícita não é autenticado por padrão
        context.User = new ClaimsPrincipal(new ClaimsIdentity()); 

        var accessor = new HttpContextAccessor { HttpContext = context };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeFalse();
        contexto.KeycloakSub.Should().BeNull();
    }

    [Fact]
    public void Usuario_Autenticado_Com_Sub_Original_Deve_Retornar_Sub()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim("sub", "12345-abc") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType")); 

        var accessor = new HttpContextAccessor { HttpContext = context };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeTrue();
        contexto.KeycloakSub.Should().Be("12345-abc");
    }

    [Fact]
    public void Usuario_Autenticado_Com_NameIdentifier_Deve_Retornar_Sub()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "98765-xyz") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType")); 

        var accessor = new HttpContextAccessor { HttpContext = context };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeTrue();
        contexto.KeycloakSub.Should().Be("98765-xyz");
    }

    [Fact]
    public void Usuario_Autenticado_Sem_Sub_Deve_Retornar_Null()
    {
        var context = new DefaultHttpContext();
        // Usuário autenticado, mas o claim "sub" ou "NameIdentifier" não existem.
        var claims = new[] { new Claim("outra_claim", "valor") };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType")); 

        var accessor = new HttpContextAccessor { HttpContext = context };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeTrue();
        contexto.KeycloakSub.Should().BeNull();
    }

    [Fact]
    public void Usuario_Autenticado_Somente_Com_Email_Username_Nao_Deve_Ser_Usados_Como_Sub()
    {
        var context = new DefaultHttpContext();
        var claims = new[] 
        { 
            new Claim(ClaimTypes.Email, "teste@teste.com"),
            new Claim("preferred_username", "teste")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType")); 

        var accessor = new HttpContextAccessor { HttpContext = context };
        var contexto = new ContextoUsuarioAutenticado(accessor);

        contexto.EstaAutenticado.Should().BeTrue();
        contexto.KeycloakSub.Should().BeNull();
    }
}
