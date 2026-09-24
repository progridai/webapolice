using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Api.Controllers.Requests;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.AtualizarModulo;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ConsultarModulo;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.CriarModulo;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.InativarModulo;
using WebApolice.Modulos.Cadastro.Application.UseCases.Modulos.ListarModulos;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/modulos")]
[Authorize]
public class ModulosController : ControllerBase
{
    private readonly ListarModulosHandler _listarHandler;
    private readonly ConsultarModuloHandler _consultarHandler;
    private readonly CriarModuloHandler _criarHandler;
    private readonly AtualizarModuloHandler _atualizarHandler;
    private readonly InativarModuloHandler _inativarHandler;

    public ModulosController(
        ListarModulosHandler listarHandler,
        ConsultarModuloHandler consultarHandler,
        CriarModuloHandler criarHandler,
        AtualizarModuloHandler atualizarHandler,
        InativarModuloHandler inativarHandler)
    {
        _listarHandler = listarHandler;
        _consultarHandler = consultarHandler;
        _criarHandler = criarHandler;
        _atualizarHandler = atualizarHandler;
        _inativarHandler = inativarHandler;
    }

    [HttpGet]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Modulos.Visualizar)]
    [ProducesResponseType(typeof(PagedResult<ModuloListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] ListarModulosQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _listarHandler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Modulos.Visualizar)]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Consultar(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _consultarHandler.Handle(new ConsultarModuloQuery { PublicId = publicId }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Modulos.Inserir)]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarModuloRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CriarModuloCommand
        {
            Nome = request.Nome,
            Descricao = request.Descricao
        };

        var result = await _criarHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(Consultar), new { publicId = result.PublicId }, result);
    }

    [HttpPut("{publicId:guid}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Modulos.Alterar)]
    [ProducesResponseType(typeof(ModuloDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid publicId,
        [FromBody] AtualizarModuloRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AtualizarModuloCommand
        {
            PublicId = publicId,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = request.Ativo
        };

        var result = await _atualizarHandler.Handle(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{publicId:guid}/inativar")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Modulos.Inativar)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(Guid publicId, CancellationToken cancellationToken)
    {
        await _inativarHandler.Handle(new InativarModuloCommand { PublicId = publicId }, cancellationToken);
        return NoContent();
    }
}
