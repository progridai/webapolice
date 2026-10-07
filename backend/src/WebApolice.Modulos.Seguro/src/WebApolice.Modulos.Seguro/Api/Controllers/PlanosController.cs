using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController, Authorize, Route("api/planos")]
public sealed class PlanosController(CatalogosSeguroHandler handler) : ControllerBase
{
    [HttpGet, AuthorizePermissao(PermissoesSeguranca.Planos.Visualizar)]
    public async Task<IActionResult> Listar(CancellationToken ct, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, [FromQuery] string? busca = null, [FromQuery] bool? ativo = null) => Ok(await handler.ListarPlanosAsync(pagina, tamanhoPagina, busca, ativo, ct));
    [HttpGet("{publicId:guid}"), AuthorizePermissao(PermissoesSeguranca.Planos.Visualizar)]
    public async Task<IActionResult> Consultar(Guid publicId, CancellationToken ct) { var result = await handler.ObterPlanoAsync(publicId, ct); return result is null ? NotFound() : Ok(result); }
    [HttpPost, AuthorizePermissao(PermissoesSeguranca.Planos.Inserir)]
    public async Task<IActionResult> Criar(PlanoDados dados, CancellationToken ct) { var result = await handler.SalvarPlanoAsync(null, dados, ct); return CreatedAtAction(nameof(Consultar), new { publicId = result.PublicId }, result); }
    [HttpPut("{publicId:guid}"), AuthorizePermissao(PermissoesSeguranca.Planos.Alterar)]
    public async Task<IActionResult> Alterar(Guid publicId, PlanoDados dados, CancellationToken ct) => Ok(await handler.SalvarPlanoAsync(publicId, dados, ct));
    [HttpPatch("{publicId:guid}/inativar"), AuthorizePermissao(PermissoesSeguranca.Planos.Inativar)]
    public async Task<IActionResult> Inativar(Guid publicId, CancellationToken ct) { await handler.StatusAsync(publicId, false, false, ct); return NoContent(); }
    [HttpPatch("{publicId:guid}/reativar"), AuthorizePermissao(PermissoesSeguranca.Planos.Reativar)]
    public async Task<IActionResult> Reativar(Guid publicId, CancellationToken ct) { await handler.StatusAsync(publicId, true, false, ct); return NoContent(); }

    [HttpGet("{publicId:guid}/coberturas"), AuthorizePermissao(PermissoesSeguranca.Planos.Visualizar)]
    public async Task<IActionResult> Coberturas(Guid publicId, CancellationToken ct) => Ok(await handler.CoberturasPlanoAsync(publicId, ct));
    [HttpPut("{publicId:guid}/coberturas/{coberturaPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.Planos.GerenciarCoberturas)]
    public async Task<IActionResult> Vincular(Guid publicId, Guid coberturaPublicId, [FromBody] PlanoCoberturaDados dados, CancellationToken ct) { await handler.SalvarVinculoAsync(publicId, coberturaPublicId, dados, ct); return NoContent(); }
    [HttpPatch("{publicId:guid}/coberturas/{coberturaPublicId:guid}/reativar"), AuthorizePermissao(PermissoesSeguranca.Planos.GerenciarCoberturas)]
    public async Task<IActionResult> ReativarVinculo(Guid publicId, Guid coberturaPublicId, CancellationToken ct) { await handler.VincularAsync(publicId, coberturaPublicId, true, ct); return NoContent(); }
    [HttpPatch("{publicId:guid}/coberturas/{coberturaPublicId:guid}/inativar"), AuthorizePermissao(PermissoesSeguranca.Planos.GerenciarCoberturas)]
    public async Task<IActionResult> Desvincular(Guid publicId, Guid coberturaPublicId, CancellationToken ct) { await handler.VincularAsync(publicId, coberturaPublicId, false, ct); return NoContent(); }
}
