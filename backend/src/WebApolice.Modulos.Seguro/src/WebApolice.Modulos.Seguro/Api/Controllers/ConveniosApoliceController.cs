using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Financeiro.Contracts;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
namespace WebApolice.Modulos.Seguro.Api.Controllers;
[ApiController,Authorize,Route("api/apolices/convenios-cobranca/opcoes")]
public sealed class ConveniosApoliceController(IConveniosCobrancaConsulta convenios):ControllerBase
{
    // Restricted to minimal selection data; financial details keep their own permission.
    [HttpGet,AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Listar(CancellationToken ct)=>Ok(await convenios.OpcoesAsync(ct));
}
