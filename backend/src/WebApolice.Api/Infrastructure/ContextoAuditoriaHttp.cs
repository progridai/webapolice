using System.Diagnostics;
using System.Security.Claims;
using WebApolice.Auditoria.Contracts;

namespace WebApolice.Api.Infrastructure;

public class ContextoAuditoriaHttp : IContextoAuditoria
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextoAuditoriaHttp(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ObterUsuarioIdExterno()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    }

    public string? ObterUsuarioNome()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value
               ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    }

    public string? ObterTraceId()
    {
        return Activity.Current?.Id ?? _httpContextAccessor.HttpContext?.TraceIdentifier;
    }

    public string? ObterCorrelationId()
    {
        if (_httpContextAccessor.HttpContext != null &&
            _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationIdValues))
        {
            var correlationId = correlationIdValues.ToString();
            
            if (string.IsNullOrWhiteSpace(correlationId))
                return null;

            // Limita o tamanho e remove caracteres de quebra de linha para evitar header injection / log injection
            correlationId = correlationId.Trim().Replace("\r", "").Replace("\n", "");
            
            if (correlationId.Length > 255)
            {
                correlationId = correlationId.Substring(0, 255);
            }

            return correlationId;
        }

        return null;
    }

    public string? ObterEnderecoIp()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    public string? ObterOrigem()
    {
        return _httpContextAccessor.HttpContext?.Request.Path.Value;
    }
}
