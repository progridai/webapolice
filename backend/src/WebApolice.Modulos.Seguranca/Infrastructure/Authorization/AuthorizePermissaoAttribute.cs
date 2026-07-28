using Microsoft.AspNetCore.Authorization;
using WebApolice.Modulos.Seguranca.Application.Authorization;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

public class AuthorizePermissaoAttribute : AuthorizeAttribute
{
    public AuthorizePermissaoAttribute(string permissao)
    {
        Policy = $"{PermissoesSeguranca.PrefixoPolicy}{permissao}";
    }
}
