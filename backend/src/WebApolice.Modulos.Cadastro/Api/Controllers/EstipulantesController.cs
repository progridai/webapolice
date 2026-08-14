using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApolice.Modulos.Cadastro.Api.Controllers.Requests;
using WebApolice.Modulos.Cadastro.Application.UseCases.AtualizarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ConsultarEstipulanteConfiguracao;
using WebApolice.Modulos.Cadastro.Application.UseCases.CriarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.InativarEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ExcluirEstipulante;
using WebApolice.Modulos.Cadastro.Application.UseCases.ListarEstipulantes;
using WebApolice.Modulos.Cadastro.Application.UseCases.ReativarEstipulante;
using WebApolice.Modulos.Seguranca.Application.Authorization;
using WebApolice.Modulos.Seguranca.Infrastructure.Authorization;

namespace WebApolice.Modulos.Cadastro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstipulantesController : ControllerBase
{
    private readonly ListarEstipulantesHandler _listarHandler;
    private readonly ConsultarEstipulantePorIdHandler _consultarPorIdHandler;
    private readonly ConsultarEstipulanteConfiguracaoHandler _consultarConfiguracaoHandler;

    public EstipulantesController(
        ListarEstipulantesHandler listarHandler,
        ConsultarEstipulantePorIdHandler consultarPorIdHandler,
        ConsultarEstipulanteConfiguracaoHandler consultarConfiguracaoHandler)
    {
        _listarHandler = listarHandler;
        _consultarPorIdHandler = consultarPorIdHandler;
        _consultarConfiguracaoHandler = consultarConfiguracaoHandler;
    }

    [HttpGet]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Visualizar)]
    public async Task<ActionResult<ListagemPaginadaResult<EstipulanteDetalheResult>>> Get(
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        [FromQuery] string? nome,
        [FromQuery] string? cnpj,
        CancellationToken cancellationToken)
    {
        var query = new ListarEstipulantesQuery(pagina, tamanhoPagina, nome, cnpj);
        var result = await _listarHandler.Handle(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{publicId}")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Visualizar)]
    public async Task<ActionResult<EstipulanteDetalheResult>> GetById(
        [FromRoute] Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new ConsultarEstipulantePorIdQuery(publicId);
        var result = await _consultarPorIdHandler.Handle(query, cancellationToken);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("{publicId}/configuracao")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Visualizar)]
    public async Task<ActionResult<EstipulanteConfiguracaoResult>> GetConfiguracao(
        [FromRoute] Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new ConsultarEstipulanteConfiguracaoQuery(publicId);
        var result = await _consultarConfiguracaoHandler.Handle(query, cancellationToken);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Inserir)]
    public async Task<IActionResult> Post([FromBody] CriarEstipulanteRequest request, [FromServices] CriarEstipulanteHandler handler, CancellationToken cancellationToken)
    {
        var command = new CriarEstipulanteCommand(
            request.RazaoSocial,
            request.NomeFantasia,
            request.Cnpj,
            request.Codigo,
            request.GrupoId,
            request.SeguradoraPublicId,
            request.Observacao,
            request.Endereco != null ? new CriarEstipulanteEnderecoCommand(request.Endereco.Cep, request.Endereco.Logradouro, request.Endereco.Numero, request.Endereco.Complemento, request.Endereco.Bairro, request.Endereco.CidadeId, request.Endereco.Uf) : null,
            request.Contatos != null ? request.Contatos.Select(c => new CriarEstipulanteContatoCommand(c.TipoContato, c.Valor, c.Principal)).ToList() : null,
            request.ContatosInstitucionais != null ? request.ContatosInstitucionais.Select(c => new CriarEstipulanteContatoInstitucionalCommand(c.Nome, c.Departamento, c.Email, c.Telefone, c.Ramal)).ToList() : null,
            new CriarEstipulanteConfiguracaoCommand(request.Configuracao.DataInicioVigencia, request.Configuracao.DataFimVigencia, request.Configuracao.Carencia, request.Configuracao.AdesaoPor, request.Configuracao.Custeio, request.Configuracao.Adesao)
        );

        var result = await handler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { publicId = result.PublicId }, result);
    }

    [HttpPut("{publicId:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Alterar)]
    public async Task<IActionResult> Put(Guid publicId, [FromBody] AtualizarEstipulanteRequest request, [FromServices] AtualizarEstipulanteHandler handler, CancellationToken cancellationToken)
    {
        var command = new AtualizarEstipulanteCommand(
            publicId,
            request.RazaoSocial,
            request.NomeFantasia,
            request.Codigo,
            request.GrupoId,
            request.SeguradoraPublicId,
            request.Observacao,
            request.Endereco != null ? new AtualizarEstipulanteEnderecoCommand(request.Endereco.Cep, request.Endereco.Logradouro, request.Endereco.Numero, request.Endereco.Complemento, request.Endereco.Bairro, request.Endereco.CidadeId, request.Endereco.Uf) : null,
            request.Contatos != null ? request.Contatos.Select(c => new AtualizarEstipulanteContatoCommand(c.TipoContato, c.Valor, c.Principal)).ToList() : null,
            request.ContatosInstitucionais != null ? request.ContatosInstitucionais.Select(c => new AtualizarEstipulanteContatoInstitucionalCommand(c.Nome, c.Departamento, c.Email, c.Telefone, c.Ramal)).ToList() : null,
            new AtualizarEstipulanteConfiguracaoCommand(request.Configuracao.DataInicioVigencia, request.Configuracao.DataFimVigencia, request.Configuracao.Carencia, request.Configuracao.AdesaoPor, request.Configuracao.Custeio, request.Configuracao.Adesao)
        );

        await handler.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{publicId:guid}/inativar")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Inativar)]
    public async Task<IActionResult> Inativar(Guid publicId, [FromServices] InativarEstipulanteHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new InativarEstipulanteCommand(publicId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{publicId:guid}")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Excluir)]
    public async Task<IActionResult> Excluir(Guid publicId, [FromServices] ExcluirEstipulanteHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new ExcluirEstipulanteCommand(publicId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{publicId:guid}/reativar")]
    [AuthorizePermissao(PermissoesSeguranca.Estipulantes.Reativar)]
    public async Task<IActionResult> Reativar(Guid publicId, [FromServices] ReativarEstipulanteHandler handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new ReativarEstipulanteCommand(publicId), cancellationToken);
        return NoContent();
    }
}
