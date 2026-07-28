using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.UseCases.Me;
using WebApolice.Modulos.Seguranca.Application.Ports;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/me")]
[Authorize]
public class MeController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Obter(
        [FromServices] WebApolice.Modulos.Seguranca.Application.UseCases.Me.ObterUsuarioAutenticadoUseCase useCase,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(cancellationToken);
        Console.WriteLine($"[DEBUG] MeController result: Encontrado={resultado.UsuarioEncontrado}, Ativo={resultado.UsuarioAtivo}, AcessoTotal={resultado.AcessoTotal}, Operador={resultado.OperadorSistema}, Modulos={string.Join(",", resultado.ModulosHabilitados)}, Permissoes={string.Join(",", resultado.Permissoes)}");
        return Ok(resultado);
    }
}
