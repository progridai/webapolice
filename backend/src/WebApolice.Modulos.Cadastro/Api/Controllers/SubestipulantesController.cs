using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarSubestipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarSubestipulantes;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarSubestipulante;
using WebApolice.Modulos.Seguranca.Application.Authorization;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/subestipulantes")]
[Route("api/cadastro/subestipulantes")]
public class SubestipulantesController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Visualizar)]
    public async Task<ActionResult<ListagemPaginadaResult<SubestipulanteListagemItemResult>>> Get(
        [FromQuery] ListarSubestipulantesQuery query,
        [FromServices] ListarSubestipulantesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Visualizar)]
    public async Task<ActionResult<SubestipulanteDetalheResult>> GetById(
        Guid publicId,
        [FromServices] ConsultarSubestipulantePorIdHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ObterSubestipulantePorPublicIdQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Inserir)]
    public async Task<ActionResult> Post(
        [FromBody] CriarSubestipulanteCommand command,
        [FromServices] CriarSubestipulanteHandler handler,
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
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Alterar)]
    public async Task<ActionResult> Put(
        Guid publicId,
        [FromBody] AlterarSubestipulanteCommand command,
        [FromServices] AlterarSubestipulanteHandler handler,
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
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Inativar)]
    public async Task<ActionResult> Inativar(
        Guid publicId,
        [FromServices] InativarSubestipulanteHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new InativarSubestipulanteCommand(publicId);
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
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Subestipulantes.Reativar)]
    public async Task<ActionResult> Reativar(
        Guid publicId,
        [FromServices] ReativarSubestipulanteHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ReativarSubestipulanteCommand(publicId);
            await handler.Handle(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
