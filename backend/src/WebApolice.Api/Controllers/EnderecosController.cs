using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.SharedKernel.Application.Ports;

namespace WebApolice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnderecosController : ControllerBase
{
    private readonly ICEPProvider _cepProvider;
    private readonly ILocalidadeResolver _localidadeResolver;
    private readonly ILogger<EnderecosController> _logger;

    public EnderecosController(ICEPProvider cepProvider, ILocalidadeResolver localidadeResolver, ILogger<EnderecosController> logger)
    {
        _cepProvider = cepProvider;
        _localidadeResolver = localidadeResolver;
        _logger = logger;
    }

    [HttpGet("cep/{cep}")]
    [ProducesResponseType(typeof(EnderecoConsultaResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ObterPorCep(string cep, CancellationToken cancellationToken)
    {
        var cepLimpo = Regex.Replace(cep ?? "", "[^0-9]", "");

        if (cepLimpo.Length != 8)
        {
            return BadRequest(new { Message = "CEP inválido. O CEP deve conter 8 dígitos." });
        }

        EnderecoProviderDto? enderecoProvedor;
        try
        {
            enderecoProvedor = await _cepProvider.ConsultarCEPAsync(cepLimpo, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            // Erro de integração com o provedor externo
            _logger.LogError(ex, "Falha no provedor de CEP. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Type = "https://webapolice/errors/provedor-cep-indisponivel",
                Title = "Serviço de endereços indisponível",
                Detail = "Não foi possível consultar o CEP. Tente novamente mais tarde.",
                Instance = Request.Path,
                Extensions = { ["traceId"] = HttpContext.TraceIdentifier }
            });
        }

        if (enderecoProvedor == null)
        {
            return NotFound(new { Message = "CEP não encontrado no provedor externo." });
        }

        var (cidadeId, estadoId) = await _localidadeResolver.ResolverLocalidadeAsync(enderecoProvedor.Cidade, enderecoProvedor.Uf, cancellationToken);

        var result = new EnderecoConsultaResult(
            Cep: enderecoProvedor.Cep,
            Logradouro: enderecoProvedor.Logradouro,
            Bairro: enderecoProvedor.Bairro,
            Cidade: enderecoProvedor.Cidade,
            Uf: enderecoProvedor.Uf,
            CidadeId: cidadeId,
            EstadoId: estadoId
        );

        return Ok(result);
    }
}
