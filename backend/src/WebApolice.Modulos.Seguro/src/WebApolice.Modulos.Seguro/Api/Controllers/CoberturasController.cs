using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Catalogos;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
namespace WebApolice.Modulos.Seguro.Api.Controllers;
[ApiController,Authorize,Route("api/coberturas")]
public sealed class CoberturasController(CatalogosSeguroHandler handler):ControllerBase
{
    [HttpGet,AuthorizePermissao(PermissoesSeguranca.Coberturas.Visualizar)]
    public async Task<IActionResult> Listar(CancellationToken ct,[FromQuery]int pagina=1,[FromQuery]int tamanhoPagina=20,[FromQuery]string? busca=null,[FromQuery]bool? ativo=null)=>Ok(await handler.ListarCoberturasAsync(pagina,tamanhoPagina,busca,ativo,ct));
    [HttpGet("{publicId:guid}"),AuthorizePermissao(PermissoesSeguranca.Coberturas.Visualizar)]
    public async Task<IActionResult> Consultar(Guid publicId,CancellationToken ct){var result=await handler.ObterCoberturaAsync(publicId,ct);return result is null?NotFound():Ok(result);}
    [HttpPost,AuthorizePermissao(PermissoesSeguranca.Coberturas.Inserir)]
    public async Task<IActionResult> Criar(CoberturaDados dados,CancellationToken ct){var result=await handler.SalvarCoberturaAsync(null,dados,ct);return CreatedAtAction(nameof(Consultar),new{publicId=result.PublicId},result);}
    [HttpPut("{publicId:guid}"),AuthorizePermissao(PermissoesSeguranca.Coberturas.Alterar)]
    public async Task<IActionResult> Alterar(Guid publicId,CoberturaDados dados,CancellationToken ct)=>Ok(await handler.SalvarCoberturaAsync(publicId,dados,ct));
    [HttpPatch("{publicId:guid}/inativar"),AuthorizePermissao(PermissoesSeguranca.Coberturas.Inativar)]
    public async Task<IActionResult> Inativar(Guid publicId,CancellationToken ct){await handler.StatusAsync(publicId,false,true,ct);return NoContent();}
    [HttpPatch("{publicId:guid}/reativar"),AuthorizePermissao(PermissoesSeguranca.Coberturas.Reativar)]
    public async Task<IActionResult> Reativar(Guid publicId,CancellationToken ct){await handler.StatusAsync(publicId,true,true,ct);return NoContent();}
}
