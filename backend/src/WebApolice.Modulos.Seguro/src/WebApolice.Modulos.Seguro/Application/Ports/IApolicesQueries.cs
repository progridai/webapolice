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
        string? buscaCliente = null,
        string? status = null,
        Guid? apoliceSubgrupoPublicId = null,
        Guid? apoliceModuloPublicId = null,
        DateOnly? vigenciaDataReferencia = null,
        CancellationToken cancellationToken = default);

    Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult?> ObterApoliceVidaPorPublicIdAsync(
        Guid apolicePublicId,
        Guid apoliceVidaPublicId,
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



    // ── Subgrupos da Apólice ────────────────────────────────────────────────
    // Subgrupo é uma divisão contextual da Apólice (não é cadastro global).

    Task<System.Collections.Generic.List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos.ApoliceSubgrupoResult>> ListarSubgruposAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken);

    Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos.ApoliceSubgrupoResult?> ObterSubgrupoPorPublicIdAsync(
        Guid apolicePublicId,
        Guid subgrupoPublicId,
        CancellationToken cancellationToken);
}

