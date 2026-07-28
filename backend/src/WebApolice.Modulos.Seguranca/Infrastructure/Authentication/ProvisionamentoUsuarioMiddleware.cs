using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authentication;

public class ProvisionamentoUsuarioMiddleware : IMiddleware
{
    private readonly IProvisionamentoUsuarioService _provisionamentoUsuarioService;

    public ProvisionamentoUsuarioMiddleware(IProvisionamentoUsuarioService provisionamentoUsuarioService)
    {
        _provisionamentoUsuarioService = provisionamentoUsuarioService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Executa o provisionamento silenciosamente, delegando ao serviço as regras de negócio
        // e verificação de autenticação
        await _provisionamentoUsuarioService.ProvisionarAsync(context.RequestAborted);
        
        await next(context);
    }
}
