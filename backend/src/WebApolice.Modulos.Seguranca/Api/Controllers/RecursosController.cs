using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Application.UseCases.Recursos;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/recursos")]
[Authorize]
public class RecursosController : ControllerBase
{
    private readonly IAcessoOperadorSistemaService _operadorSistemaService;
    private readonly IContextoUsuarioAutenticado _contexto;

    public RecursosController(IAcessoOperadorSistemaService operadorSistemaService, IContextoUsuarioAutenticado contexto)
    {
        _operadorSistemaService = operadorSistemaService;
        _contexto = contexto;
    }

    private async Task<bool> ValidarOperadorAsync(CancellationToken cancellationToken)
    {
        return await _operadorSistemaService.EhOperadorSistemaAsync(_contexto.KeycloakSub!, cancellationToken);
    }

    [HttpPut("{publicId:guid}/habilitacao")]
    public async Task<IActionResult> AlterarHabilitacao(
        Guid publicId,
        [FromBody] AlterarHabilitacaoRecursoRequest request,
        [FromServices] AlterarHabilitacaoRecursoUseCase useCase,
        CancellationToken cancellationToken)
    {
        if (!await ValidarOperadorAsync(cancellationToken)) return Forbid();
        await useCase.ExecuteAsync(new AlterarHabilitacaoRecursoCommand(publicId, request.Habilitado), cancellationToken);
        return NoContent();
    }
}

public record AlterarHabilitacaoRecursoRequest(bool Habilitado);
