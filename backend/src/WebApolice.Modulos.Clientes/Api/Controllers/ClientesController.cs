using System.Collections.Generic;
using System.Linq;
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
            request.TipoPessoa,
            request.Nome,
            request.Documento,
            request.DataNascimento,
            request.Sexo,
            request.Observacao,
            request.Falecido,
            request.DataObito,
            request.Contatos?.Select(c => new ContatoCommand(c.TipoContato, c.Valor, c.Principal)).ToList() ?? new List<ContatoCommand>(),
            request.Enderecos?.Select(e => new EnderecoCommand(
                e.TipoEndereco,
                e.Cep,
                e.Logradouro,
                e.Numero,
                e.Complemento,
                e.Bairro,
                e.CidadeId == 0 ? null : e.CidadeId,
                e.Uf,
                e.Principal
            )).ToList() ?? new List<EnderecoCommand>()
        );

        var result = await handler.Handle(command, UsuarioSub, cancellationToken);
        
        return CreatedAtAction(nameof(ObterPorId), new { id = result.PublicId }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PoliticasAutorizacao.ConsultaClientes)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] System.Guid id,
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
        [FromQuery] string? documento,
        [FromQuery] short? status_id,
        [FromQuery] string? ordenar_por,
        [FromQuery] string? direcao,
        [FromServices] ListarClientesHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarClientesQuery(pagina, tamanho_pagina, nome, documento, status_id, ordenar_por, direcao);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Alterar(
        [FromRoute] System.Guid id,
        [FromBody] AlterarClienteRequest request,
        [FromServices] AlterarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AlterarClienteCommand(
            id,
            request.Nome,
            request.Documento,
            request.DataNascimento,
            request.Sexo,
            request.Observacao,
            request.Falecido,
            request.DataObito,
            request.Contatos?.Select(c => new ContatoCommand(c.TipoContato, c.Valor, c.Principal)).ToList() ?? new List<ContatoCommand>(),
            request.Enderecos?.Select(e => new EnderecoCommand(
                e.TipoEndereco,
                e.Cep,
                e.Logradouro,
                e.Numero,
                e.Complemento,
                e.Bairro,
                e.CidadeId == 0 ? null : e.CidadeId,
                e.Uf,
                e.Principal
            )).ToList() ?? new List<EnderecoCommand>()
        );

        await handler.Handle(command, UsuarioSub, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/ativar")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Ativar(
        [FromRoute] System.Guid id,
        [FromServices] AtivarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new AtivarClienteCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/inativar")]
    [Authorize(Policy = PoliticasAutorizacao.GestaoClientes)]
    public async Task<IActionResult> Inativar(
        [FromRoute] System.Guid id,
        [FromServices] InativarClienteHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new InativarClienteCommand(id), cancellationToken);
        return NoContent();
    }
}
