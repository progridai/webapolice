using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguro.Api.Controllers.Requests;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarRamo;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.CriarRamo;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;
using WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ObterRamo;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RamosController : ControllerBase
{
    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Visualizar)]
    public async Task<IActionResult> Get(
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        [FromServices] ListarRamosHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarRamosQuery(pagina, tamanhoPagina, busca, ativo);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Visualizar)]
    public async Task<IActionResult> GetById(
        Guid publicId,
        [FromServices] ObterRamoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ObterRamoQuery(publicId), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Inserir)]
    public async Task<IActionResult> Post(
        [FromBody] CriarRamoRequest request,
        [FromServices] CriarRamoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CriarRamoCommand(request.Codigo, request.Nome, request.Descricao);
        var result = await handler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { publicId = result.PublicId }, result);
    }

    [HttpPut("{publicId:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Alterar)]
    public async Task<IActionResult> Put(
        Guid publicId,
        [FromBody] AlterarRamoRequest request,
        [FromServices] AlterarRamoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AlterarRamoCommand(publicId, request.Nome, request.Descricao);
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{publicId:guid}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Inativar)]
    public async Task<IActionResult> Inativar(
        Guid publicId,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo.AlterarStatusRamoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo.AlterarStatusRamoCommand(publicId, false);
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{publicId:guid}/reativar")]
    [AuthorizePermissao(PermissoesSeguranca.Ramos.Reativar)]
    public async Task<IActionResult> Reativar(
        Guid publicId,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo.AlterarStatusRamoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarStatusRamo.AlterarStatusRamoCommand(publicId, true);
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
}
