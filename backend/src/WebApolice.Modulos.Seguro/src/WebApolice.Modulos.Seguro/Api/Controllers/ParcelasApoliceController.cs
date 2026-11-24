using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Contracts.Authorization;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.Financeiro;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/apolices/{apolicePublicId:guid}/parcelas")]
[Authorize]
public sealed class ParcelasApoliceController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    public async Task<IActionResult> Conferir(Guid apolicePublicId, [FromQuery] int competencia, CancellationToken ct) =>
        Ok(await mediator.Send(new ConferirLoteQuery(apolicePublicId, competencia), ct));
    [HttpPost("lote")]
    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    public async Task<IActionResult> Gerar(Guid apolicePublicId, [FromBody] LoteMensalDados dados, CancellationToken ct) =>
        Ok(await mediator.Send(new GerarLoteCommand(apolicePublicId, dados), ct));
    [HttpPost("fechamento")]
    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    public async Task<IActionResult> Fechar(Guid apolicePublicId, [FromBody] LoteMensalDados dados, CancellationToken ct) =>
        Ok(await mediator.Send(new FecharLoteCommand(apolicePublicId, dados), ct));
    [HttpPost("{parcelaPublicId:guid}/baixa")]
    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    public async Task<IActionResult> Baixar(Guid apolicePublicId, Guid parcelaPublicId, [FromBody] BaixarParcelaDados dados, CancellationToken ct) =>
        Ok(await mediator.Send(new BaixarParcelaCommand(apolicePublicId, parcelaPublicId, dados), ct));
}
