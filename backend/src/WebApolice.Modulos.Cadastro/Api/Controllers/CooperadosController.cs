using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Api.Requests;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Cadastro.Application.UseCases.CadastrarCooperado;
using WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCooperado;
using WebApolice.Modulos.Cadastro.Application.UseCases.AtivarCooperado;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarCooperado;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarCooperado;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarCooperados;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/cooperados")]
[Authorize]
public sealed class CooperadosController : ControllerBase
{
    private string UsuarioSub => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "sistema";

    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Visualizar)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina,
        [FromQuery] int tamanho_pagina,
        [FromQuery] string? nome,
        [FromQuery] string? cpf,
        [FromQuery] short? status,
        [FromQuery] string? ordenar_por,
        [FromQuery] string? direcao,
        [FromServices] ListarCooperadosHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarCooperadosQuery(pagina, tamanho_pagina, nome ?? cpf, status);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Visualizar)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] Guid id,
        [FromServices] ConsultarCooperadoHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ConsultarCooperadoQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Inserir)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarCooperadoRequest request,
        [FromServices] CadastrarCooperadoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CadastrarCooperadoCommand(
            request.Nome,
            request.Cpf,
            request.DataNascimento,
            request.Telefone,
            request.Email,
            request.Cep,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.CidadeId,
            request.Uf,
            request.Tipo,
            request.Codigo,
            request.Rg,
            request.OrgaoEmissor,
            request.DataEmissaoRg,
            request.Susep,
            request.Inss,
            request.Issqn,
            request.NumeroDependentes,
            request.DataInscricao,
            request.Credenciado,
            request.CoordenadorId,
            request.BancoId,
            request.Agencia,
            request.ContaCorrente,
            request.Observacao
        );

        var result = await handler.Handle(command, UsuarioSub, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.PublicId }, result);
    }

    [HttpPut("{id:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Alterar)]
    public async Task<IActionResult> Alterar(
        [FromRoute] Guid id,
        [FromBody] AlterarCooperadoRequest request,
        [FromServices] AlterarCooperadoHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new AlterarCooperadoCommand(
            id,
            request.Nome,
            request.DataNascimento,
            request.Telefone,
            request.Email,
            request.Cep,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.CidadeId,
            request.Uf,
            request.Codigo,
            request.Rg,
            request.OrgaoEmissor,
            request.DataEmissaoRg,
            request.Susep,
            request.Inss,
            request.Issqn,
            request.NumeroDependentes,
            request.DataInscricao,
            request.Credenciado,
            request.CoordenadorId,
            request.BancoId,
            request.Agencia,
            request.ContaCorrente,
            request.Observacao
        );

        await handler.Handle(command, UsuarioSub, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Inativar)]
    public async Task<IActionResult> Inativar(
        [FromRoute] Guid id,
        [FromServices] InativarCooperadoHandler handler,
        [FromQuery] DateOnly? dataDesligamento,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new InativarCooperadoCommand(id, dataDesligamento ?? DateOnly.FromDateTime(DateTime.UtcNow)), UsuarioSub, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/ativar")]
    [AuthorizePermissao(PermissoesSeguranca.Cooperados.Reativar)]
    public async Task<IActionResult> Ativar(
        [FromRoute] Guid id,
        [FromServices] AtivarCooperadoHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new AtivarCooperadoCommand(id), UsuarioSub, cancellationToken);
        return NoContent();
    }
}
