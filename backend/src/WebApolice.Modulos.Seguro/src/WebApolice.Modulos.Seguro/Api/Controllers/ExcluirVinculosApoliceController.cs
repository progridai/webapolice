using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Application.Ports;
using WebApolice.Modulos.Seguranca.Contracts.Authorization;
using WebApolice.Modulos.Seguro.Application.Ports;
namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController, Authorize, Route("api/apolices/{apolicePublicId:guid}/vinculos")]
public sealed class ExcluirVinculosApoliceController(IExclusaoVinculosApolice exclusao, IContextoUsuarioAutenticado usuario) : ControllerBase
{
    [HttpDelete("ramos/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesRamos.Excluir)]
    public Task<IActionResult> Ramos(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.Ramo, vinculoPublicId, ct);

    [HttpDelete("subgrupos/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesSubgrupos.Excluir)]
    public Task<IActionResult> Subgrupos(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.Subgrupo, vinculoPublicId, ct);

    [HttpDelete("subestipulantes/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesSubestipulantes.Excluir)]
    public Task<IActionResult> Subestipulantes(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.Subestipulante, vinculoPublicId, ct);

    [HttpDelete("modulos/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Excluir)]
    public Task<IActionResult> Modulos(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.Modulo, vinculoPublicId, ct);

    [HttpDelete("planos-modulo/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Excluir)]
    public Task<IActionResult> PlanosModulo(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.PlanoModulo, vinculoPublicId, ct);

    [HttpDelete("coberturas-modulo/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesModulos.Excluir)]
    public Task<IActionResult> CoberturasModulo(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.CoberturaModulo, vinculoPublicId, ct);

    [HttpDelete("produtos-anteriores/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesUniverso.Excluir)]
    public Task<IActionResult> ProdutosAnteriores(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.ProdutoAnterior, vinculoPublicId, ct);

    [HttpDelete("planos-anteriores/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesUniverso.Excluir)]
    public Task<IActionResult> PlanosAnteriores(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.PlanoAnterior, vinculoPublicId, ct);

    [HttpDelete("coberturas-anteriores/{vinculoPublicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ApolicesUniverso.Excluir)]
    public Task<IActionResult> CoberturasAnteriores(Guid apolicePublicId, Guid vinculoPublicId, CancellationToken ct) =>
        Excluir(apolicePublicId, TipoVinculoApolice.CoberturaAnterior, vinculoPublicId, ct);

    private async Task<IActionResult> Excluir(Guid apolice, TipoVinculoApolice tipo, Guid vinculo, CancellationToken ct)
    {
        await exclusao.ExcluirAsync(apolice, tipo, vinculo, Guid.Parse(usuario.KeycloakSub ?? Guid.Empty.ToString()), ct);
        return NoContent();
    }
}
