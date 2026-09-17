using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
    private readonly IMediator _mediator;

    public ApoliceModulosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Listar(Guid apolicePublicId, CancellationToken cancellationToken)
    {
        var query = new ListarModulosApoliceQuery { ApolicePublicId = apolicePublicId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{apoliceModuloPublicId}")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Obter(Guid apolicePublicId, Guid apoliceModuloPublicId, CancellationToken cancellationToken)
    {
        var query = new ObterModuloApolicePorPublicIdQuery 
        { 
            ApolicePublicId = apolicePublicId, 
            ApoliceModuloPublicId = apoliceModuloPublicId 
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Inserir)]
    public async Task<IActionResult> Criar(
        Guid apolicePublicId, 
        [FromBody] CriarModuloApoliceRequest request,
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

        var newPublicId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { PublicId = newPublicId });
    }

    [HttpPut("{apoliceModuloPublicId}")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Alterar)]
    public async Task<IActionResult> Alterar(
        Guid apolicePublicId, 
        Guid apoliceModuloPublicId, 
        [FromBody] AlterarModuloApoliceRequest request,
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

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{apoliceModuloPublicId}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Inativar)]
    public async Task<IActionResult> Inativar(
        Guid apolicePublicId, 
        Guid apoliceModuloPublicId,
        [FromServices] IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new InativarModuloApoliceCommand
        {
            ApolicePublicId = apolicePublicId,
            ApoliceModuloPublicId = apoliceModuloPublicId,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public class CriarModuloApoliceRequest
{
    public Guid ModuloPublicId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacao { get; set; }
}

public class AlterarModuloApoliceRequest
{
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Observacao { get; set; }
}
