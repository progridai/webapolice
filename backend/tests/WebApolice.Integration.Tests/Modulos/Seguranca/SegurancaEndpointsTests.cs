using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Api.Controllers;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Seguranca;

public class SegurancaEndpointsTests
{
    [Fact]
    public void UsuariosController_DeveEstarProtegidoPorPoliciesCorretas()
    {
        var type = typeof(UsuariosController);
        
        var getMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpGetAttribute>() != null);
        foreach (var method in getMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.usuarios.visualizar", auth.Policy);
        }

        var postMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null);
        foreach (var method in postMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.usuarios.inserir", auth.Policy);
        }

        var putMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPutAttribute>() != null);
        foreach (var method in putMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.usuarios.alterar", auth.Policy);
        }
    }

    [Fact]
    public void PerfisController_DeveEstarProtegidoPorPoliciesCorretas()
    {
        var type = typeof(PerfisController);
        
        var getMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpGetAttribute>() != null);
        foreach (var method in getMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.perfis.visualizar", auth.Policy);
        }

        var postMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null);
        foreach (var method in postMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.perfis.inserir", auth.Policy);
        }

        var putMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPutAttribute>() != null);
        foreach (var method in putMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:seguranca.perfis.alterar", auth.Policy);
        }
    }

    [Fact]
    public void MeController_NaoDeveExigirPermissoesAdministrativas()
    {
        var type = typeof(MeController);
        
        // Verifica se a classe tem apenas [Authorize] sem policy
        var classAuth = type.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Null(classAuth.Policy);

        // Verifica se o método não sobrescreve com policy mais restrita
        var method = type.GetMethod("ObterContextoAtual");
        var methodAuth = method?.GetCustomAttribute<AuthorizeAttribute>();
        Assert.Null(methodAuth); // Nao deve ter [Authorize] no método se já tem na classe sem policy, ou se tiver, não deve ter policy administrativa
    }
}
