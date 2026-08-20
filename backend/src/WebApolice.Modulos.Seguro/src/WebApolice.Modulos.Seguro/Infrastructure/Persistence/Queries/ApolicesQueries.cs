using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Models;

namespace WebApolice.Modulos.Seguro.Infrastructure.Persistence.Queries;

public class ApolicesQueries : IApolicesQueries
{
    private readonly SeguroDbContext _dbContext;

    public ApolicesQueries(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ApoliceListagemItemResult>> ListarPaginadoAsync(
        int pagina,
        int tamanhoPagina,
        string? busca,
        string? status,
        bool? ativo,
        Guid? estipulanteId,
        Guid? seguradoraId,
        string? tipoRamo,
        DateTime? vigenciaDataReferencia,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var b = busca.ToLower();
            query = query.Where(a => a.Nome != null && a.Nome.ToLower().Contains(b));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status == status);
        }

        if (ativo.HasValue)
        {
            query = query.Where(a => a.Ativo == ativo.Value);
        }

        var totalItens = await query.CountAsync(cancellationToken);
        
        var skip = (pagina - 1) * tamanhoPagina;

        var itens = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(a => new ApoliceListagemItemResult(
                a.PublicId,
                a.Nome ?? "",
                "Estipulante Nome (Implementar)",
                "Seguradora Nome (Implementar)",
                a.DataInicioVigencia,
                a.DataFimVigencia,
                a.Status,
                a.Ativo,
                a.Ramos.Count,
                ""
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ApoliceListagemItemResult> 
        {
            Items = itens, 
            Page = pagina, 
            PageSize = tamanhoPagina, 
            TotalCount = totalItens
        };
    }

    public async Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceDetalheResult?> ObterDetalhePorPublicIdAsync(
        Guid publicId, 
        CancellationToken cancellationToken)
    {
        var apolice = await _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.PublicId == publicId && a.DeletedAt == null)
            .Select(a => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceDetalheResult(
                a.PublicId,
                a.Nome ?? "",
                a.EstipulanteId,
                "Estipulante Nome (Implementar)",
                a.SeguradoraId,
                "Seguradora Nome (Implementar)",
                a.CorretoraId,
                "Corretora Nome (Implementar)",
                a.DataInicioVigencia,
                a.DataFimVigencia,
                a.DataAniversario,
                a.Status,
                a.Ativo,
                a.Observacao,
                a.Ramos.Select(ar => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceRamoResult(
                    ar.Ramo.PublicId,
                    ar.Ramo.Codigo,
                    ar.Ramo.Nome,
                    ar.NumeroApolice,
                    ar.IofPercentual,
                    ar.Ativo
                )).ToList(),
                a.Configuracao != null ? new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceConfiguracaoResult(
                    a.Configuracao.TipoAdesao,
                    a.Configuracao.Custeio,
                    a.Configuracao.CarenciaDias,
                    a.Configuracao.MesBaseReajuste,
                    a.Configuracao.IndiceReajuste,
                    a.Configuracao.CobreConjuge,
                    a.Configuracao.ControlaExcedente,
                    a.Configuracao.DiaCorteFaturamento,
                    a.Configuracao.PrazoAvisoSinistroDias
                ) : null
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return apolice;
    }

    public async Task<PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>> ListarVidasPaginadoAsync(
        Guid apolicePublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .Where(a => a.PublicId == apolicePublicId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0)
        {
            return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>
            { 
                Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(),
                Page = pagina, 
                PageSize = tamanhoPagina, 
                TotalCount = 0 
            };
        }

        var query = _dbContext.ApoliceVidas
            .AsNoTracking()
            .Where(v => v.ApoliceId == apoliceId && v.DeletedAt == null);

        var totalItens = await query.CountAsync(cancellationToken);
        
        var skip = (pagina - 1) * tamanhoPagina;

        var itens = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(v => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult(
                v.PublicId,
                v.ClienteId,
                $"Cliente {v.ClienteId} (Implementar JOIN)",
                "000.000.000-00",
                v.ApoliceSubestipulanteId,
                v.ApoliceSubestipulanteId != null ? $"Subestipulante {v.ApoliceSubestipulanteId}" : null,
                null, // WORKAROUND: a coluna apolice_subestipulante_modulo_id nÃ£o existe no DB
                null,
                v.DataInicioVigencia,
                v.DataFimVigencia,
                v.Status,
                v.Ativo
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>
        {
            Items = itens,
            Page = pagina, 
            PageSize = tamanhoPagina, 
            TotalCount = totalItens
        };
    }

    public async Task<List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult>> ListarSubestipulantesAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .Where(a => a.PublicId == apolicePublicId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0) return new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult>();

        var itens = await _dbContext.ApoliceSubestipulantes
            .AsNoTracking()
            .Where(s => s.ApoliceId == apoliceId && s.DeletedAt == null)
            .Select(s => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult(
                s.SubestipulanteId,
                s.DataInicio,
                s.DataFim,
                s.Ativo,
                s.Modulos
                 .Where(m => m.DeletedAt == null)
                 .Select(m => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteModuloResult(
                    m.ModuloId,
                    m.DataInicio,
                    m.DataFim,
                    m.Ativo
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return itens;
    }

    public async Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceUniversoPermitidoResult?> ObterUniversoPermitidoAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .Where(a => a.PublicId == apolicePublicId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0) return null;

        var produtos = await _dbContext.ApoliceProdutos
            .AsNoTracking()
            .Where(p => p.ApoliceId == apoliceId)
            .Select(p => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceProdutoResult(
                p.ProdutoId,
                p.Ativo,
                p.Planos.Select(pl => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApolicePlanoResult(
                    pl.PlanoId,
                    pl.TabelaPrecoId,
                    pl.Ativo,
                    pl.Coberturas.Select(c => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceCoberturaResult(
                        c.CoberturaId,
                        c.Ativo,
                        c.ImportanciaSeguradaOverride,
                        c.PremioOverride
                    )).ToList()
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterUniversoPermitido.ApoliceUniversoPermitidoResult(produtos);
    }

    public async Task<PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult>> ListarHistoricoPaginadoAsync(
        Guid apolicePublicId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .Where(a => a.PublicId == apolicePublicId)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0)
        {
            return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult>
            { 
                Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult>(),
                Page = pagina, 
                PageSize = tamanhoPagina, 
                TotalCount = 0 
            };
        }

        var query = _dbContext.ApoliceHistoricos
            .AsNoTracking()
            .Where(h => h.ApoliceId == apoliceId);

        var totalItens = await query.CountAsync(cancellationToken);
        
        var skip = (pagina - 1) * tamanhoPagina;

        var itens = await query
            .OrderByDescending(h => h.DataAcao)
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(h => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult(
                h.Acao,
                h.Descricao,
                h.UsuarioPublicId,
                h.DataAcao
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarHistorico.ApoliceHistoricoResult>
        {
            Items = itens,
            Page = pagina, 
            PageSize = tamanhoPagina, 
            TotalCount = totalItens
        };
    }
}
