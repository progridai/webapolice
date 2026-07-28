using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

public sealed class PermissaoAuthorizationHandler : AuthorizationHandler<PermissaoRequirement>
{
    private readonly IContextoUsuarioAutenticado _contextoUsuarioAutenticado;
    private readonly IPermissoesEfetivasService _permissoesEfetivasService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Persistence.SegurancaDbContext _dbContext;

    public PermissaoAuthorizationHandler(
        IContextoUsuarioAutenticado contextoUsuarioAutenticado,
        IPermissoesEfetivasService permissoesEfetivasService,
        IHttpContextAccessor httpContextAccessor,
        Persistence.SegurancaDbContext dbContext)
    {
        _contextoUsuarioAutenticado = contextoUsuarioAutenticado;
        _permissoesEfetivasService = permissoesEfetivasService;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissaoRequirement requirement)
    {
        if (!_contextoUsuarioAutenticado.EstaAutenticado)
        {
            return; // Irá resultar em negado caso nenhuma outra policy o aprove
        }

        var keycloakSub = _contextoUsuarioAutenticado.KeycloakSub;

        if (string.IsNullOrWhiteSpace(keycloakSub))
        {
            return;
        }

        // Obtém o token de cancelamento a partir da requisição HTTP (se disponível)
        var cancellationToken = _httpContextAccessor.HttpContext?.RequestAborted ?? default;

        // Verifica o módulo ao qual a permissão pertence
        var modulo = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            System.Linq.Queryable.Select(
                System.Linq.Queryable.Where(_dbContext.Permissoes, p => p.Codigo == requirement.CodigoPermissao),
                p => new { p.Recurso.Modulo.Codigo, p.Recurso.Modulo.Habilitado }
            ), cancellationToken);

        if (modulo != null && !modulo.Habilitado)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = 403;
                _httpContextAccessor.HttpContext.Response.ContentType = "application/problem+json; charset=utf-8";
                await _httpContextAccessor.HttpContext.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Type = "https://webapolice/errors/modulo-nao-habilitado",
                    Title = "Módulo não habilitado",
                    Status = 403,
                    Detail = $"O módulo '{modulo.Codigo}' está desabilitado no sistema."
                });
                await _httpContextAccessor.HttpContext.Response.Body.FlushAsync();
            }
            context.Fail();
            return;
        }

        // O serviço trata de devolver os booleanos UsuarioEncontrado e UsuarioAtivo
        var permissoesEfetivas = await _permissoesEfetivasService.CalcularPermissoesAsync(keycloakSub, cancellationToken);

        if (!permissoesEfetivas.UsuarioEncontrado || !permissoesEfetivas.UsuarioAtivo)
        {
            return;
        }

        if (permissoesEfetivas.AcessoTotal || permissoesEfetivas.Permissoes.Contains(requirement.CodigoPermissao, StringComparer.Ordinal))
        {
            context.Succeed(requirement);
        }
    }
}
