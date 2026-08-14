using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using WebApolice.Modulos.Cadastro.Api.Controllers;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using Xunit;

namespace WebApolice.Integration.Tests.Modulos.Clientes;

public class ClientesAuthorizationTests
{
    [Fact]
    public void Endpoints_De_Clientes_Devem_Usar_AuthorizePermissao_E_Nao_AuthorizePolicy_Antigo()
    {
        var controllerType = typeof(ClientesController);
        var metodos = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (var metodo in metodos)
        {
            // Ignora propriedades que geram mÃ©todos get_UsuarioSub etc.
            if (metodo.IsSpecialName) continue;

            var attributes = metodo.GetCustomAttributes(true);
            
            var authorizeAntigoComPolicy = attributes.OfType<AuthorizeAttribute>()
                .Where(a => a.GetType() == typeof(AuthorizeAttribute) && !string.IsNullOrEmpty(a.Policy));

            // NÃ£o deve ter [Authorize(Policy = "...")]
            Assert.Empty(authorizeAntigoComPolicy);

            var authorizePermissao = attributes.OfType<AuthorizePermissaoAttribute>().ToList();
            
            // Deve ter [AuthorizePermissao(...)]
            Assert.Single(authorizePermissao);

            var policyGerada = authorizePermissao.First().Policy;
            Assert.StartsWith("Permissao:clientes.", policyGerada);
        }
    }
}
