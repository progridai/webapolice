using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/auditoria")]
[Authorize]
public class AuditoriaController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permissao:seguranca.auditoria.visualizar")]
    public async Task<IActionResult> Listar(
        [FromServices] ListarAuditoriaUseCase useCase,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await useCase.ExecuteAsync(pagina, tamanhoPagina, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = "Permissao:seguranca.auditoria.visualizar")]
    public async Task<IActionResult> Obter(
        [FromServices] ObterAuditoriaUseCase useCase,
        [FromRoute] Guid publicId,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(publicId, cancellationToken);
        if (resultado == null) return NotFound();
        return Ok(resultado);
    }
}
