using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCorretora;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarCorretora;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarCorretora;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarCorretora;
using WebApolice.Modulos.Seguranca.Application.Authorization;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/corretoras")]
[Authorize]
public class CorretorasController : ControllerBase
{
    private readonly ICorretorasQueries _queries;
    private readonly CriarCorretoraHandler _criarHandler;
    private readonly AlterarCorretoraHandler _alterarHandler;
    private readonly InativarCorretoraHandler _inativarHandler;
    private readonly ReativarCorretoraHandler _reativarHandler;

    public CorretorasController(
        ICorretorasQueries queries,
        CriarCorretoraHandler criarHandler,
        AlterarCorretoraHandler alterarHandler,
        InativarCorretoraHandler inativarHandler,
        ReativarCorretoraHandler reativarHandler)
    {
        _queries = queries;
        _criarHandler = criarHandler;
        _alterarHandler = alterarHandler;
        _inativarHandler = inativarHandler;
        _reativarHandler = reativarHandler;
    }

    [HttpGet]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Visualizar)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? busca = null,
        [FromQuery] bool? ativo = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _queries.ListarPaginadoAsync(pagina, tamanhoPagina, busca, ativo, cancellationToken);
        
        return Ok(new
        {
            itens = result.itens,
            totalItens = result.totalItens,
            totalPaginas = result.totalPaginas
        });
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Visualizar)]
    public async Task<IActionResult> ObterPorId(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _queries.ObterPorPublicIdAsync(publicId, cancellationToken);
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Inserir)]
    public async Task<IActionResult> Criar([FromBody] CriarCorretoraCommand command, CancellationToken cancellationToken)
    {
        var publicId = await _criarHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { publicId }, new { publicId });
    }

    [HttpPut("{publicId:guid}")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Alterar)]
    public async Task<IActionResult> Alterar(Guid publicId, [FromBody] AlterarCorretoraCommand command, CancellationToken cancellationToken)
    {
        if (publicId != command.PublicId)
            return BadRequest("O ID da rota não corresponde ao corpo da requisição.");

        await _alterarHandler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{publicId:guid}/inativar")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Inativar)]
    public async Task<IActionResult> Inativar(Guid publicId, CancellationToken cancellationToken)
    {
        await _inativarHandler.Handle(new InativarCorretoraCommand { PublicId = publicId }, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{publicId:guid}/reativar")]
    [Authorize(Policy = PermissoesSeguranca.PrefixoPolicy + PermissoesSeguranca.Corretoras.Reativar)]
    public async Task<IActionResult> Reativar(Guid publicId, CancellationToken cancellationToken)
    {
        await _reativarHandler.Handle(new ReativarCorretoraCommand { PublicId = publicId }, cancellationToken);
        return NoContent();
    }
}
