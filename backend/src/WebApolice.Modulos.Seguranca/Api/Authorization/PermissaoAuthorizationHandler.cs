using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Api.Authorization;

public sealed class PermissaoAuthorizationHandler : AuthorizationHandler<PermissaoRequirement>
{
    private readonly IContextoUsuarioAutenticado _contextoUsuarioAutenticado;
    private readonly IPermissoesEfetivasService _permissoesEfetivasService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WebApolice.Modulos.Seguranca.Contracts.IDisponibilidadeCatalogo _catalogo;

    public PermissaoAuthorizationHandler(
        IContextoUsuarioAutenticado contextoUsuarioAutenticado,
        IPermissoesEfetivasService permissoesEfetivasService,
        IHttpContextAccessor httpContextAccessor,
        WebApolice.Modulos.Seguranca.Contracts.IDisponibilidadeCatalogo catalogo)
    {
        _contextoUsuarioAutenticado = contextoUsuarioAutenticado;
        _permissoesEfetivasService = permissoesEfetivasService;
        _httpContextAccessor = httpContextAccessor;
        _catalogo = catalogo;
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

        // Catálogo inexistente, inativo ou desabilitado nunca é liberado por AcessoTotal.
        var disponivel = await _catalogo.PermissaoDisponivelAsync(requirement.CodigoPermissao, cancellationToken);
        if (!disponivel)
        {
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
