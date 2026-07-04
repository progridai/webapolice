using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Shared.Infrastructure.Security;
using WebApolice.Modulos.Clientes.Api.Requests;
using WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.AtivarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.CadastrarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.InativarCliente;
using WebApolice.Modulos.Clientes.Application.UseCases.ListarClientes;
using WebApolice.Modulos.Clientes.Domain;

namespace WebApolice.Modulos.Clientes.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public sealed class ClientesController : ControllerBase
{
    private string UsuarioSub => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "sistema";

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarClienteRequest request,
        [FromServices] CadastrarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CadastrarClienteCommand(
            request.Nome,
            request.Cpf,
            request.DataNascimento,
            request.Email,
            request.Telefone,
            request.CodigoLegado);

        var result = await handler.Handle(command, UsuarioSub, cancellationToken);
        
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PoliticasAutorizacao.ConsultaClientes)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] long id,
        [FromServices] ConsultarClientePorIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ConsultarClientePorIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = PoliticasAutorizacao.ConsultaClientes)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina,
        [FromQuery] int tamanho_pagina,
        [FromQuery] string? nome,
        [FromQuery] string? cpf,
        [FromQuery] StatusCliente? status,
        [FromQuery] string? ordenar_por,
        [FromQuery] string? direcao,
        [FromServices] ListarClientesHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarClientesQuery(pagina, tamanho_pagina, nome, cpf, status, ordenar_por, direcao);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Alterar(
        [FromRoute] long id,
        [FromBody] AlterarClienteRequest request,
        [FromServices] AlterarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AlterarClienteCommand(
            id,
            request.Nome,
            request.DataNascimento,
            request.Email,
            request.Telefone);

        await handler.Handle(command, UsuarioSub, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/ativar")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Ativar(
        [FromRoute] long id,
        [FromServices] AtivarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new AtivarClienteCommand(id), UsuarioSub, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/inativar")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Inativar(
        [FromRoute] long id,
        [FromServices] InativarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new InativarClienteCommand(id), UsuarioSub, cancellationToken);
        return NoContent();
    }
}
