using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authentication;

public class ContextoUsuarioAutenticado : IContextoUsuarioAutenticado
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextoUsuarioAutenticado(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool EstaAutenticado => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? KeycloakSub
    {
        get
        {
            if (!EstaAutenticado)
                return null;

            var principal = _httpContextAccessor.HttpContext!.User;

            var sub = principal.FindFirst("sub")?.Value 
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return !string.IsNullOrEmpty(sub) ? sub : null;
        }
    }

    public string? Username
    {
        get
        {
            if (!EstaAutenticado) return null;
            var val = _httpContextAccessor.HttpContext!.User.FindFirst("preferred_username")?.Value;
            return !string.IsNullOrEmpty(val) ? val : null;
        }
    }

    public string? Nome
    {
        get
        {
            if (!EstaAutenticado) return null;
            var val = _httpContextAccessor.HttpContext!.User.FindFirst("name")?.Value;
            return !string.IsNullOrEmpty(val) ? val : null;
        }
    }

    public string? Email
    {
        get
        {
            if (!EstaAutenticado) return null;
            var val = _httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.Email)?.Value
                      ?? _httpContextAccessor.HttpContext!.User.FindFirst("email")?.Value;
            return !string.IsNullOrEmpty(val) ? val : null;
        }
    }
}
