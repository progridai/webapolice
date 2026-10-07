using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController, Authorize, Route("api/apolices/{apolicePublicId:guid}/coberturas/{vinculoPublicId:guid}/premios")]
public sealed class PremiosApoliceController(PremiosApoliceHandler handler) : ControllerBase
{
    [HttpPut, AuthorizePermissao(PermissoesSeguranca.Apolices.Alterar)]
    public async Task<IActionResult> Alterar(Guid apolicePublicId, Guid vinculoPublicId, [FromBody] PremiosCoberturaDados dados, CancellationToken ct)
    {
        await handler.AlterarAsync(apolicePublicId, vinculoPublicId, dados, ct);
        return NoContent();
    }
}
