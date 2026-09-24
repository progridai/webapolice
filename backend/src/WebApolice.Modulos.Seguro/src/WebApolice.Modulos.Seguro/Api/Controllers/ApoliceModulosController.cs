using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModuloApolice;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/apolices/{apolicePublicId}/modulos")]
[Authorize]
public class ApoliceModulosController : ControllerBase
{
    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Listar(
        Guid apolicePublicId,
        [FromServices] ListarModulosApoliceHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarModulosApoliceQuery { ApolicePublicId = apolicePublicId };
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{apoliceModuloPublicId}")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Obter(
        Guid apolicePublicId,
        Guid apoliceModuloPublicId,
        [FromServices] ObterModuloApolicePorPublicIdHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ObterModuloApolicePorPublicIdQuery
        {
            ApolicePublicId = apolicePublicId,
            ApoliceModuloPublicId = apoliceModuloPublicId
        };
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Inserir)]
    public async Task<IActionResult> Criar(
        Guid apolicePublicId,
        [FromBody] CriarModuloApoliceRequest request,
        [FromServices] CriarModuloApoliceHandler handler,
        [FromServices] IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new CriarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            ModuloPublicId = request.ModuloPublicId,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Observacao = request.Observacao,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };

        var newPublicId = await handler.Handle(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { PublicId = newPublicId });
    }

    [HttpPut("{apoliceModuloPublicId}")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Alterar)]
    public async Task<IActionResult> Alterar(
        Guid apolicePublicId,
        Guid apoliceModuloPublicId,
        [FromBody] AlterarModuloApoliceRequest request,
        [FromServices] AlterarModuloApoliceHandler handler,
        [FromServices] IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new AlterarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            ApoliceModuloPublicId = apoliceModuloPublicId,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Observacao = request.Observacao,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };

        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{apoliceModuloPublicId}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Inativar)]
    public async Task<IActionResult> Inativar(
        Guid apolicePublicId,
        Guid apoliceModuloPublicId,
        [FromServices] InativarModuloApoliceHandler handler,
        [FromServices] IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new InativarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            ApoliceModuloPublicId = apoliceModuloPublicId,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };

        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
}

public class CriarModuloApoliceRequest
{
    public Guid ModuloPublicId { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Observacao { get; set; }
}

public class AlterarModuloApoliceRequest
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Observacao { get; set; }
}
