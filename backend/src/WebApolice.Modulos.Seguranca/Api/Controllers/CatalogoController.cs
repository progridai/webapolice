using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/catalogo")]
[Authorize]
public class CatalogoController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permissao:seguranca.catalogo.visualizar")]
    public async Task<IActionResult> Listar(
        [FromServices] ListarCatalogoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("permissoes")]
    [Authorize(Policy = "Permissao:seguranca.catalogo.visualizar")]
    public async Task<IActionResult> ListarPermissoes(
        [FromServices] ListarPermissoesUseCase useCase,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(cancellationToken);
        return Ok(resultado);
    }
}
