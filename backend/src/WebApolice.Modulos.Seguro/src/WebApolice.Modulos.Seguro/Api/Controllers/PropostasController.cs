using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/propostas")]
[Authorize]
public class PropostasController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropostasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromQuery] ObterPropostasQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaCommand command)
    {
        var resultId = await _mediator.Send(command);
        return Created($"/api/propostas/{resultId}", new { PublicId = resultId });
    }

    [HttpPost("{publicId:guid}/itens")]
    public async Task<IActionResult> AdicionarItem(Guid publicId, [FromBody] AdicionarPropostaItemCommand command)
    {
        command.PropostaId = publicId;
        var resultId = await _mediator.Send(command);
        return Ok(new { ItemId = resultId });
    }
}
