using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/modulos")]
[Authorize]
public class ModulosController : ControllerBase
{
    private readonly IAcessoOperadorSistemaService _operadorSistemaService;
    private readonly IContextoUsuarioAutenticado _contexto;

    public ModulosController(IAcessoOperadorSistemaService operadorSistemaService, IContextoUsuarioAutenticado contexto)
    {
        _operadorSistemaService = operadorSistemaService;
        _contexto = contexto;
    }

    private async Task<bool> ValidarOperadorAsync(CancellationToken cancellationToken)
    {
        return await _operadorSistemaService.EhOperadorSistemaAsync(_contexto.KeycloakSub!, cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromServices] ListarModulosUseCase useCase, CancellationToken cancellationToken)
    {
        if (!await ValidarOperadorAsync(cancellationToken)) return Forbid();
        var modulos = await useCase.ExecuteAsync(cancellationToken);
        return Ok(modulos);
    }

    [HttpPut("{publicId:guid}/habilitacao")]
    public async Task<IActionResult> AlterarHabilitacao(
        Guid publicId,
        [FromBody] AlterarHabilitacaoRequest request,
        [FromServices] AlterarHabilitacaoModuloUseCase useCase,
        CancellationToken cancellationToken)
    {
        if (!await ValidarOperadorAsync(cancellationToken)) return Forbid();
        await useCase.ExecuteAsync(new AlterarHabilitacaoModuloCommand(publicId, request.Habilitado), cancellationToken);
        return NoContent();
    }
}

public record AlterarHabilitacaoRequest(bool Habilitado);
