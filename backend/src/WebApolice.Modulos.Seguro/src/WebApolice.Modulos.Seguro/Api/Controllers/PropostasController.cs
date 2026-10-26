using WebApolice.Modulos.Seguranca.Contracts.Authorization;
using WebApolice.Modulos.Seguranca.Application.Authorization;
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

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromQuery] ObterPropostasQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Inserir)]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaCommand command, CancellationToken cancellationToken)
    {
        var resultId = await _mediator.Send(command, cancellationToken);
        return Created($"/api/propostas/{resultId}", new { PublicId = resultId });
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/itens")]
    public async Task<IActionResult> AdicionarItem(Guid publicId, [FromBody] AdicionarPropostaItemCommand command, CancellationToken cancellationToken)
    {
        command.PropostaId = publicId;
        var resultId = await _mediator.Send(command, cancellationToken);
        return Ok(new { ItemId = resultId });
    }
}
