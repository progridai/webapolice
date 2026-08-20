using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;
using WebApolice.Modulos.Seguro.Api.Controllers.Requests;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApolicesController : ControllerBase
{
    public ApolicesController()
    {
    }

    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> Get(
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        [FromQuery] string? busca,
        [FromQuery] string? status,
        [FromQuery] bool? ativo,
        [FromQuery] Guid? estipulanteId,
        [FromQuery] Guid? seguradoraId,
        [FromQuery] string? tipoRamo,
        [FromQuery] DateTime? vigenciaDataReferencia,
        [FromServices] ListarApolicesHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new ListarApolicesQuery(
            pagina, 
            tamanhoPagina, 
            busca, 
            status, 
            ativo, 
            estipulanteId, 
            seguradoraId, 
            tipoRamo, 
            vigenciaDataReferencia);

        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Inserir)]
    public async Task<IActionResult> Post(
        [FromBody] CriarApoliceRequest request, 
        [FromServices] CriarApoliceHandler handler, 
        CancellationToken cancellationToken)
    {
        var command = new CriarApoliceCommand(
            request.EstipulanteId,
            request.SeguradoraId,
            request.CorretoraId,
            request.Nome,
            request.DataInicioVigencia,
            request.DataFimVigencia,
            request.DataAniversario,
            request.SubestipulantesIds,
            request.Observacao
        );

        var result = await handler.Handle(command, cancellationToken);
        
        return CreatedAtAction("GetById", new { publicId = result.PublicId }, result);
    }

    [HttpPut("{publicId:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Alterar)]
    public async Task<IActionResult> Put(
        Guid publicId,
        [FromBody] AlterarApoliceRequest request,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApolice.AlterarApoliceHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarApolice.AlterarApoliceCommand(
            publicId,
            request.EstipulanteId,
            request.SeguradoraId,
            request.CorretoraId,
            request.Nome,
            request.DataInicioVigencia,
            request.DataFimVigencia,
            request.DataAniversario,
            request.Observacao
        );

        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
    
    [HttpGet("{publicId}")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> GetById(
        Guid publicId,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ObterApoliceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ObterApolicePorPublicIdQuery(publicId), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{publicId}/vidas")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> GetVidas(
        Guid publicId,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ListarApoliceVidasHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ListarApoliceVidasQuery(publicId, pagina, tamanhoPagina);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}/subestipulantes")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> GetSubestipulantes(
        Guid publicId,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ListarApoliceSubestipulantesHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ListarApoliceSubestipulantesQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}/universo-permitido")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> GetUniversoPermitido(
        Guid publicId,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ObterApoliceUniversoPermitidoHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ObterApoliceUniversoPermitidoQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{publicId}/historico")]
    [AuthorizePermissao(PermissoesSeguranca.Apolices.Visualizar)]
    public async Task<IActionResult> GetHistorico(
        Guid publicId,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        [FromServices] WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ListarApoliceHistoricoHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ListarApoliceHistoricoQuery(publicId, pagina, tamanhoPagina);
        var result = await handler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{publicId}/ramos")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesRamos.Inserir)]
    public async Task<IActionResult> PostRamo(
        Guid publicId,
        [FromBody] VincularRamoApoliceRequest request,
        [FromServices] WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo.VincularRamoApoliceHandler handler,
        [FromServices] WebApolice.Modulos.Seguranca.Application.Ports.IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularRamo.VincularRamoApoliceCommand
        {
            ApolicePublicId = publicId,
            RamoPublicId = request.RamoPublicId,
            NumeroApolice = request.NumeroApolice,
            IofPercentual = request.IofPercentual,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };
        await handler.Handle(command, cancellationToken);
        return Ok();
    }

    [HttpPut("{publicId}/ramos/{ramoPublicId}")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesRamos.Alterar)]
    public async Task<IActionResult> PutRamo(
        Guid publicId,
        Guid ramoPublicId,
        [FromBody] AtualizarRamoApoliceRequest request,
        [FromServices] WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarRamo.AtualizarRamoApoliceHandler handler,
        [FromServices] WebApolice.Modulos.Seguranca.Application.Ports.IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarRamo.AtualizarRamoApoliceCommand
        {
            ApolicePublicId = publicId,
            RamoPublicId = ramoPublicId,
            NumeroApolice = request.NumeroApolice,
            IofPercentual = request.IofPercentual,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{publicId}/ramos/{ramoPublicId}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.ApolicesRamos.Inativar)]
    public async Task<IActionResult> PatchInativarRamo(
        Guid publicId,
        Guid ramoPublicId,
        [FromServices] WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo.InativarRamoApoliceHandler handler,
        [FromServices] WebApolice.Modulos.Seguranca.Application.Ports.IContextoUsuarioAutenticado userContext,
        CancellationToken cancellationToken)
    {
        var command = new WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarRamo.InativarRamoApoliceCommand
        {
            ApolicePublicId = publicId,
            RamoPublicId = ramoPublicId,
            UsuarioPublicId = Guid.Parse(userContext.KeycloakSub ?? Guid.Empty.ToString())
        };
        await handler.Handle(command, cancellationToken);
        return NoContent();
    }
}
