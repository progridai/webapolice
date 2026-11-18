using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EncerrarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.SimularValorMensal;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.RevisarCondicoes;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ReincluirProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.GerirAgendamento;
using WebApolice.Modulos.Seguranca.Contracts.Authorization;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using System;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ConfirmarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.EfetivarVigencia;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.PreencherRascunho;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ListarPropostas;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas.ObterProposta;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/propostas")]
[Authorize]
public class PropostasController : ControllerBase
{
    private readonly IMediator _mediator;

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    [HttpGet("{publicId:guid}/simulacao-mensal/opcoes")]
    public async Task<IActionResult> OpcoesSimulacao(Guid publicId, CancellationToken ct) =>
        Ok(await _mediator.Send(new OpcoesSimulacaoMensalQuery(publicId), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    [HttpPost("{publicId:guid}/simulacao-mensal")]
    public async Task<IActionResult> SimularMensal(Guid publicId, [FromBody] SimulacaoMensalDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new SimularValorMensalQuery(publicId, dados), ct));

    public PropostasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpGet("{publicId:guid}/condicoes/opcoes")]
    public async Task<IActionResult> OpcoesCondicoes(Guid publicId, CancellationToken ct) =>
        Ok(await _mediator.Send(new OpcoesCondicoesQuery(publicId), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/condicoes/comparacao")]
    public async Task<IActionResult> CompararCondicoes(Guid publicId, [FromBody] SelecaoCondicoes dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new CompararCondicoesQuery(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/condicoes/aplicar")]
    public async Task<IActionResult> AplicarCondicoes(Guid publicId, [FromBody] AplicarCondicoesDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new AplicarCondicoesCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromQuery] ObterPropostasQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Visualizar)]
    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> Obter(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterPropostaQuery(publicId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
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

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPut("{publicId:guid}/rascunho")]
    public async Task<IActionResult> Preencher(Guid publicId, [FromBody] PreencherRascunhoDados dados, CancellationToken cancellationToken)
    {
        var revisao = await _mediator.Send(new PreencherRascunhoCommand(publicId, dados), cancellationToken);
        return Ok(new { RevisaoPreenchimento = revisao });
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/confirmacao")]
    public async Task<IActionResult> Confirmar(Guid publicId, [FromBody] ConfirmarPropostaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new ConfirmarPropostaCommand(publicId, dados), ct));

    public sealed record EfetivarVigenciaDados(int? RevisaoEsperada);

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/vigencia/efetivar")]
    public async Task<IActionResult> EfetivarVigencia(Guid publicId, [FromBody] EfetivarVigenciaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new EfetivarVigenciaPropostaCommand(publicId, dados.RevisaoEsperada), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/agendamento/reagendar")]
    public async Task<IActionResult> Reagendar(Guid publicId, [FromBody] ReagendarPropostaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new ReagendarPropostaCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/agendamento/cancelar")]
    public async Task<IActionResult> CancelarAgendamento(Guid publicId, [FromBody] CancelarAgendamentoDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new CancelarAgendamentoCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/vigencia/cancelamento")]
    public async Task<IActionResult> SolicitarCancelamento(Guid publicId, [FromBody] CancelarVigenciaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new SolicitarCancelamentoVigenciaCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/vigencia/cancelamento/efetivar")]
    public async Task<IActionResult> EfetivarCancelamento(Guid publicId, [FromBody] EfetivarVigenciaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new EfetivarCancelamentoVigenciaCommand(publicId, dados.RevisaoEsperada), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/vigencia/cancelamento/alterar")]
    public async Task<IActionResult> AlterarCancelamento(Guid publicId, [FromBody] CancelarVigenciaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new AlterarCancelamentoVigenciaCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/vigencia/cancelamento/retirar")]
    public async Task<IActionResult> RetirarCancelamento(Guid publicId, [FromBody] RetirarCancelamentoVigenciaDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new RetirarCancelamentoVigenciaCommand(publicId, dados), ct));

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Alterar)]
    [HttpPost("{publicId:guid}/rascunho/descarte")]
    public async Task<IActionResult> DescartarRascunho(Guid publicId, [FromBody] DescartarRascunhoDados dados, CancellationToken ct) =>
        Ok(await _mediator.Send(new DescartarRascunhoCommand(publicId, dados), ct));

    public sealed record CriarDaVidaRequest(Guid? PropostaOrigemPublicId);

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Inserir)]
    [HttpPost("{publicId:guid}/reinclusao")]
    public async Task<IActionResult> PrepararReinclusao(Guid publicId, [FromBody] PrepararReinclusaoDados dados, CancellationToken ct)
    {
        var id = await _mediator.Send(new PrepararReinclusaoCommand(publicId, dados), ct);
        return Created($"/api/propostas/{id}", new { PublicId = id });
    }

    [AuthorizePermissao(PermissoesSeguranca.Propostas.Inserir)]
    [HttpPost("/api/apolices/{apolicePublicId:guid}/vidas/{vidaPublicId:guid}/propostas")]
    public async Task<IActionResult> CriarDaVida(Guid apolicePublicId, Guid vidaPublicId, [FromBody] CriarDaVidaRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CriarPropostaDaVidaCommand(apolicePublicId, vidaPublicId, request.PropostaOrigemPublicId), cancellationToken);
        return Created($"/api/propostas/{id}", new { PublicId = id });
    }
}
