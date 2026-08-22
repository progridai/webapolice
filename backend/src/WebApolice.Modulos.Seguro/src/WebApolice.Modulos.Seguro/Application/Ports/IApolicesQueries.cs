using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Application.Ports;

public interface IApolicesQueries
{
    Task<PagedResult<ApoliceListagemItemResult>> ListarPaginadoAsync(
        int pagina,
        int tamanhoPagina,
        string? busca,
        string? status,
        bool? ativo,
        Guid? estipulanteId,
        Guid? seguradoraId,
        string? tipoRamo,
        DateTime? vigenciaDataReferencia,
        CancellationToken cancellationToken);

    Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceDetalheResult?> ObterDetalhePorPublicIdAsync(
        Guid publicId, 
        CancellationToken cancellationToken);
        
    Task<PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>> ListarVidasPaginadoAsync(
        Guid apolicePublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);
        
    Task<System.Collections.Generic.List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult>> ListarSubestipulantesAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken);
        
    Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceUniversoPermitidoResult?> ObterUniversoPermitidoAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken);
        
    Task<PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult>> ListarHistoricoPaginadoAsync(
        Guid apolicePublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);

    Task<System.Collections.Generic.List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos.ModuloDoSubestipulanteResult>> ListarModulosDoSubestipulanteAsync(
        Guid apolicePublicId,
        Guid subestipulantePublicId,
        CancellationToken cancellationToken);
}
