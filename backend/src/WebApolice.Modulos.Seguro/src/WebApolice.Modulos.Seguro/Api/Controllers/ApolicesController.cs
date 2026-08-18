using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Seguro.Api.Controllers.Requests;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarApolice;

namespace WebApolice.Modulos.Seguro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApolicesController : ControllerBase
{
    public ApolicesController()
    {
    }

    [HttpPost]
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
            request.Ramos?.Select(r => new CriarApoliceRamoCommand(r.TipoRamo, r.NumeroApolice, r.IofPercentual)).ToList(),
            request.SubestipulantesIds,
            request.Observacao
        );

        var result = await handler.Handle(command, cancellationToken);
        
        // Retornamos 201 Created apontando para o endpoint GetById (que implementaremos no proximo passo)
        return CreatedAtAction("GetById", new { publicId = result.PublicId }, result);
    }
    
    [HttpGet("{publicId}")]
    public IActionResult GetById(Guid publicId)
    {
        // Placeholder para a consulta implementada no proximo passo
        return Ok(new { PublicId = publicId, Status = "Pendente implementação do Handler de Consulta" });
    }
}
