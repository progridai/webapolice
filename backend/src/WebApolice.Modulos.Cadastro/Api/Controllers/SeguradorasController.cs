using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSeguradora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSeguradora;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarSeguradora;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarSeguradora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarSeguradoras;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSeguradora;
using WebApolice.Modulos.Seguranca.Application.Authorization;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/seguradoras")]
[Route("api/cadastro/seguradoras")]
public class SeguradorasController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Visualizar)]
    public async Task<ActionResult<ListagemPaginadaResult<SeguradoraListagemItemResult>>> Get(
        [FromQuery] ListarSeguradorasQuery query,
        [FromServices] ListarSeguradorasHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Visualizar)]
    public async Task<ActionResult<SeguradoraDetalheResult>> GetById(
        Guid publicId,
        [FromServices] ConsultarSeguradoraPorIdHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ObterSeguradoraPorIdQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Inserir)]
    public async Task<ActionResult> Post(
        [FromBody] CriarSeguradoraCommand command,
        [FromServices] CriarSeguradoraHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var publicId = await handler.Handle(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { publicId }, new { publicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{publicId}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Alterar)]
    public async Task<ActionResult> Put(
        Guid publicId,
        [FromBody] AlterarSeguradoraCommand command,
        [FromServices] AlterarSeguradoraHandler handler,
        CancellationToken cancellationToken)
    {
        if (publicId != command.PublicId)
            return BadRequest(new { message = "ID da rota não confere com o ID do corpo da requisição." });

        try
        {
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{publicId}/inativar")]
    [HttpPatch("{publicId}/inativar")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Inativar)]
    public async Task<ActionResult> Inativar(
        Guid publicId,
        [FromServices] InativarSeguradoraHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new InativarSeguradoraCommand(publicId);
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{publicId}/reativar")]
    [HttpPatch("{publicId}/reativar")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Seguradoras.Reativar)]
    public async Task<ActionResult> Reativar(
        Guid publicId,
        [FromServices] ReativarSeguradoraHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ReativarSeguradoraCommand(publicId);
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
