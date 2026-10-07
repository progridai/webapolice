using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.PlanoModulo;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController, Authorize, Route("api/apolices/{apolicePublicId:guid}/modulos/{apoliceModuloPublicId:guid}/plano")]
public sealed class PlanoModuloApoliceController(PlanoModuloApoliceHandler handler) : ControllerBase
{
    [HttpGet, AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Obter(Guid apolicePublicId, Guid apoliceModuloPublicId, CancellationToken ct) =>
        Ok(await handler.ObterAsync(apolicePublicId, apoliceModuloPublicId, ct));

    [HttpPut, AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Alterar)]
    public async Task<IActionResult> SalvarPlano(Guid apolicePublicId, Guid apoliceModuloPublicId, [FromBody] PlanoModuloDados dados, CancellationToken ct) =>
        Ok(await handler.SalvarPlanoAsync(apolicePublicId, apoliceModuloPublicId, dados, ct));

    [HttpGet("coberturas/opcoes"), AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> OpcoesCoberturas(Guid apolicePublicId, Guid apoliceModuloPublicId, CancellationToken ct,
        [FromQuery] int pagina = 1, [FromQuery] int tamanho = 100) =>
        Ok(await handler.OpcoesCoberturasAsync(apolicePublicId, apoliceModuloPublicId, pagina, tamanho, ct));

    [HttpPost("coberturas"), AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Alterar)]
    public async Task<IActionResult> VincularCobertura(Guid apolicePublicId, Guid apoliceModuloPublicId, [FromBody] CoberturaModuloDados dados, CancellationToken ct) =>
        StatusCode(201, await handler.SalvarCoberturaAsync(apolicePublicId, apoliceModuloPublicId, null, dados, ct));

    [HttpPut("coberturas/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Alterar)]
    public async Task<IActionResult> AlterarCobertura(Guid apolicePublicId, Guid apoliceModuloPublicId, Guid vinculoPublicId,
        [FromBody] CoberturaModuloDados dados, CancellationToken ct) =>
        Ok(await handler.SalvarCoberturaAsync(apolicePublicId, apoliceModuloPublicId, vinculoPublicId, dados, ct));
}
