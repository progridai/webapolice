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
    private readonly IMediator _mediator;

    public ModulosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ModuloListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarModulosQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CriarModuloRequest request)
    {
        var command = new CriarModuloCommand
        {
            Nome = request.Nome,
            Descricao = request.Descricao
        };

        var result = await _mediator.Send(command);
        return Created($"/api/modulos/{result.PublicId}", result);
    }

    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Atualizar([FromRoute] Guid publicId, [FromBody] AtualizarModuloRequest request)
    {
        var command = new WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo.AtualizarModuloCommand
        {
            PublicId = publicId,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = request.Ativo
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Inativar([FromRoute] Guid publicId)
    {
        var command = new WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo.InativarModuloCommand
        {
            PublicId = publicId
        };

        await _mediator.Send(command);
        return NoContent();
    }
}
