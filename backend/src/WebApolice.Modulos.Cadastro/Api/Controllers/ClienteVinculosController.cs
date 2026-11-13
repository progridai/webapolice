using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Application.UseCases.Vinculos;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Contracts.Authorization;
namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController, Authorize, Route("api/clientes/{clienteId:guid}/vinculos")]
public sealed class ClienteVinculosController : ControllerBase
{
    [HttpGet, AuthorizePermissao(PermissoesSeguranca.Clientes.Visualizar)]
    public async Task<IActionResult> Listar(Guid clienteId, [FromServices] ClienteVinculosHandler handler,
        CancellationToken ct, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20) =>
        Ok(await handler.ListarAsync(clienteId, pagina, tamanhoPagina, ct));

    [HttpGet("estipulantes-disponiveis"), AuthorizePermissao(PermissoesSeguranca.Clientes.Visualizar)]
    public async Task<IActionResult> Estipulantes(Guid clienteId, [FromServices] ClienteVinculosHandler handler,
        CancellationToken ct, [FromQuery] string? busca = null, [FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20) =>
        Ok(await handler.EstipulantesAsync(clienteId, busca, pagina, tamanhoPagina, ct));

    [HttpPost, AuthorizePermissao(PermissoesSeguranca.Clientes.Alterar)]
    public async Task<IActionResult> Criar(Guid clienteId, [FromBody] SalvarClienteVinculo dados,
        [FromServices] ClienteVinculosHandler handler, CancellationToken ct) =>
        Ok(new { publicId = await handler.SalvarAsync(clienteId, null, dados, ct) });

    public sealed record SituacaoVinculo(bool Ativo);
    [HttpPatch("{publicId:guid}/situacao"), AuthorizePermissao(PermissoesSeguranca.Clientes.Alterar)]
    public async Task<IActionResult> Situacao(Guid clienteId, Guid publicId, [FromBody] SituacaoVinculo dados,
        [FromServices] ClienteVinculosHandler handler, CancellationToken ct)
    { await handler.SituacaoAsync(clienteId, publicId, dados.Ativo, ct); return NoContent(); }

    [HttpPut("{publicId:guid}"), AuthorizePermissao(PermissoesSeguranca.Clientes.Alterar)]
    public async Task<IActionResult> Alterar(Guid clienteId, Guid publicId, [FromBody] SalvarClienteVinculo dados,
        [FromServices] ClienteVinculosHandler handler, CancellationToken ct)
    { await handler.SalvarAsync(clienteId, publicId, dados, ct); return NoContent(); }
}
