using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Estipulantes.Api.Controllers;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Estipulantes;

public class EstipulantesEndpointsTests
{
    [Fact]
    public void EstipulantesController_DeveEstarProtegidoPorPoliciesCorretas()
    {
        var type = typeof(EstipulantesController);
        
        // Verifica se o controller tem [Authorize] na classe
        var authorizeController = type.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authorizeController);

        var getMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpGetAttribute>() != null);
        foreach (var method in getMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizePermissaoAttribute>();
            Assert.NotNull(auth);
            Assert.Equal("Permissao:estipulantes.visualizar", auth.Policy);
        }

        var postMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPostAttribute>() != null);
        foreach (var method in postMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizePermissaoAttribute>();
            Assert.NotNull(auth);
            
            // Tratamento especial pois no Estipulantes temos dois POSTs além do inserção genérica:
            var httpPost = method.GetCustomAttribute<HttpPostAttribute>();
            if (httpPost?.Template == "{publicId:guid}/inativar")
                Assert.Equal("Permissao:estipulantes.inativar", auth.Policy);
            else if (httpPost?.Template == "{publicId:guid}/reativar")
                Assert.Equal("Permissao:estipulantes.reativar", auth.Policy);
            else
                Assert.Equal("Permissao:estipulantes.inserir", auth.Policy);
        }

        var putMethods = type.GetMethods().Where(m => m.GetCustomAttribute<HttpPutAttribute>() != null);
        foreach (var method in putMethods)
        {
            var auth = method.GetCustomAttribute<AuthorizePermissaoAttribute>();
            Assert.NotNull(auth);
            // Todos os PUTs (Estipulante e Configuracao) exigem a mesma permissão: alterar
            Assert.Equal("Permissao:estipulantes.alterar", auth.Policy);
        }
    }
}
