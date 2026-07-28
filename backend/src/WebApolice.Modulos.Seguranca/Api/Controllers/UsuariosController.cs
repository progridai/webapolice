using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Api.Requests;
using WebApolice.Modulos.Seguranca.Application.UseCases.Usuarios;

namespace WebApolice.Modulos.Seguranca.Api.Controllers;

[ApiController]
[Route("api/seguranca/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permissao:seguranca.usuarios.visualizar")]
    public async Task<IActionResult> Listar(
        [FromServices] ListarUsuariosUseCase useCase,
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        [FromQuery] Guid? perfil,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resultado = await useCase.ExecuteAsync(busca, ativo, perfil, pagina, tamanhoPagina, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = "Permissao:seguranca.usuarios.visualizar")]
    public async Task<IActionResult> Obter(
        [FromServices] ObterUsuarioUseCase useCase,
        [FromRoute] Guid publicId,
        CancellationToken cancellationToken)
    {
        var resultado = await useCase.ExecuteAsync(publicId, cancellationToken);
        if (resultado == null) return NotFound();
        return Ok(resultado);
    }

    [HttpPost]
    [Authorize(Policy = "Permissao:seguranca.usuarios.inserir")]
    public async Task<IActionResult> Criar(
        [FromServices] CriarUsuarioUseCase useCase,
        [FromBody] CriarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var id = await useCase.ExecuteAsync(
            request.Username,
            request.Nome,
            request.Email,
            request.SenhaTemporaria,
            request.Ativo,
            request.PerfilPublicIds ?? new System.Collections.Generic.List<Guid>(),
            cancellationToken);

        return CreatedAtAction(nameof(Obter), new { publicId = id }, new { id });
    }

    [HttpPut("{publicId:guid}")]
    [Authorize(Policy = "Permissao:seguranca.usuarios.alterar")]
    public async Task<IActionResult> Atualizar(
        [FromServices] AtualizarUsuarioUseCase useCase,
        [FromRoute] Guid publicId,
        [FromBody] AtualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(
            publicId,
            request.Nome,
            request.Email,
            request.Ativo,
            request.PerfilPublicIds ?? new System.Collections.Generic.List<Guid>(),
            cancellationToken);

        return NoContent();
    }
}
