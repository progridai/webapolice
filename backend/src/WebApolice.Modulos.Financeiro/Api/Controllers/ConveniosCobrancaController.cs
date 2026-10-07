using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Financeiro.Application.UseCases.ConveniosCobranca;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
namespace WebApolice.Modulos.Financeiro.Api.Controllers;
[ApiController, Authorize, Route("api/convenios-cobranca")]
public sealed class ConveniosCobrancaController(ConveniosCobrancaHandler handler) : ControllerBase
{
    [HttpGet, AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Visualizar)]
    public async Task<IActionResult> Listar(CancellationToken ct,[FromQuery]int pagina=1,[FromQuery]int tamanhoPagina=20,[FromQuery]string? busca=null,[FromQuery]bool? ativo=null)=>Ok(await handler.ListarAsync(pagina,tamanhoPagina,busca,ativo,ct));
    [HttpGet("bancos"), AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Visualizar)]
    public async Task<IActionResult> Bancos(CancellationToken ct)=>Ok(await handler.BancosAsync(ct));
    [HttpGet("{publicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Visualizar)]
    public async Task<IActionResult> Consultar(Guid publicId,CancellationToken ct)
    {
        var result=await handler.ConsultarAsync(publicId,ct); return result is null?NotFound():Ok(result);
    }
    [HttpPost, AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Inserir)]
    public async Task<IActionResult> Criar(ConvenioCobrancaDados dados,CancellationToken ct)
    {
        var result=await handler.SalvarAsync(null,dados,ct);return CreatedAtAction(nameof(Consultar),new { publicId=result.PublicId },result);
    }
    [HttpPut("{publicId:guid}"), AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Alterar)]
    public async Task<IActionResult> Alterar(Guid publicId,ConvenioCobrancaDados dados,CancellationToken ct)=>Ok(await handler.SalvarAsync(publicId,dados,ct));
    [HttpPatch("{publicId:guid}/inativar"), AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Inativar)]
    public async Task<IActionResult> Inativar(Guid publicId,CancellationToken ct){await handler.AlterarStatusAsync(publicId,false,ct);return NoContent();}
    [HttpPatch("{publicId:guid}/reativar"), AuthorizePermissao(PermissoesSeguranca.ConveniosCobranca.Reativar)]
    public async Task<IActionResult> Reativar(Guid publicId,CancellationToken ct){await handler.AlterarStatusAsync(publicId,true,ct);return NoContent();}
}
