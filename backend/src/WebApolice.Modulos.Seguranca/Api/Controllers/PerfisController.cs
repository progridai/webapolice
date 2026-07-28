using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Api.Requests;
using WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/perfis")]
[Authorize]
public class PerfisController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permissao:seguranca.perfis.visualizar")]
    public async Task<IActionResult> Listar(
        [FromServices] ListarPerfisUseCase useCase,
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await useCase.ExecuteAsync(busca, ativo, pagina, tamanhoPagina, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = "Permissao:seguranca.perfis.visualizar")]
    public async Task<IActionResult> Obter(
        [FromServices] ObterPerfilUseCase useCase,
        [FromRoute] Guid publicId,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(publicId, cancellationToken);
        if (resultado == null) return NotFound();
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Policy = "Permissao:seguranca.perfis.inserir")]
    public async Task<IActionResult> Criar(
        [FromServices] CriarPerfilUseCase useCase,
        [FromBody] CriarPerfilRequest request,
        CancellationToken cancellationToken)
    {
        var id = await useCase.ExecuteAsync(
            request.Codigo,
            request.Nome,
            request.Descricao,
            request.Ativo,
            request.PermissaoPublicIds ?? new System.Collections.Generic.List<Guid>(),
            cancellationToken);

        return CreatedAtAction(nameof(Obter), new { publicId = id }, new { id });
    }

    [HttpPut("{publicId:guid}")]
    [Authorize(Policy = "Permissao:seguranca.perfis.alterar")]
    public async Task<IActionResult> Atualizar(
        [FromServices] AtualizarPerfilUseCase useCase,
        [FromRoute] Guid publicId,
        [FromBody] AtualizarPerfilRequest request,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(
            publicId,
            request.Nome,
            request.Descricao,
            request.Ativo,
            request.PermissaoPublicIds ?? new System.Collections.Generic.List<Guid>(),
            cancellationToken);

        return NoContent();
    }
}
