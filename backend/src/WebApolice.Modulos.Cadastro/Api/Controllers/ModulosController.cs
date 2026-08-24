using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Api.Controllers.Requests;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/modulos")]
[Authorize]
public class ModulosController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ModuloListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] ListarModulosQuery query,
        [FromServices] ListarModulosHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarModuloRequest request,
        [FromServices] CriarModuloHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CriarModuloCommand
        {
            Nome = request.Nome,
            Descricao = request.Descricao
        };

        var result = await handler.Handle(command, cancellationToken);
        return Created($"/api/modulos/{result.PublicId}", result);
    }

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Atualizar(
        [FromRoute] Guid publicId, 
        [FromBody] AtualizarModuloRequest request,
        [FromServices] WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo.AtualizarModuloHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo.AtualizarModuloCommand
        {
            PublicId = publicId,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = request.Ativo
        };

        var result = await handler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Inativar(
        [FromRoute] Guid publicId,
        [FromServices] WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo.InativarModuloHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo.InativarModuloCommand
        {
            PublicId = publicId
        };

        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
}
