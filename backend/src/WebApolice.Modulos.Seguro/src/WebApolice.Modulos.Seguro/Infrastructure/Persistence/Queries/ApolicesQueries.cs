using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;

using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubgrupos;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Models;
using System.Data.Common;

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

        var apolicesRaw = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(a => new
            {
                a.PublicId,
                a.Nome,
                a.EstipulanteId,
                a.SeguradoraId,
                a.CorretoraId,
                a.DataInicioVigencia,
                a.DataFimVigencia,
                a.Status,
                a.Ativo,
                QtdRamos = a.Ramos.Count
            })
            .ToListAsync(cancellationToken);

        var estipulanteIds = apolicesRaw.Select(a => a.EstipulanteId).Distinct().ToList();
        var seguradoraIds = apolicesRaw.Select(a => a.SeguradoraId).Distinct().ToList();
        var corretoraIds = apolicesRaw.Where(a => a.CorretoraId.HasValue).Select(a => a.CorretoraId!.Value).Distinct().ToList();

        var nomesEstipulantes = await ObterNomesGlobaisAsync("estipulante", estipulanteIds, cancellationToken);
        var nomesSeguradoras = await ObterNomesGlobaisAsync("seguradora", seguradoraIds, cancellationToken);
        var nomesCorretoras = await ObterNomesGlobaisAsync("corretora", corretoraIds, cancellationToken);

        var itens = apolicesRaw.Select(a => new ApoliceListagemItemResult(
            a.PublicId,
            a.Nome ?? "",
            nomesEstipulantes.GetValueOrDefault(a.EstipulanteId, "Desconhecido"),
            nomesSeguradoras.GetValueOrDefault(a.SeguradoraId, "Desconhecido"),
            a.DataInicioVigencia,
            a.DataFimVigencia,
            a.Status,
            a.Ativo,
            a.QtdRamos,
            ""
        )).ToList();

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
        var apoliceRaw = await _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.PublicId == publicId && a.DeletedAt == null)
            .Select(a => new
            {
                a.PublicId,
                a.Nome,
                a.EstipulanteId,
                a.SeguradoraId,
                a.CorretoraId,
                a.DataInicioVigencia,
                a.DataFimVigencia,
                a.DataAniversario,
                a.Status,
                a.Ativo,
                a.Observacao,
                Ramos = a.Ramos.Select(ar => new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceRamoResult(
                    ar.Ramo.PublicId,
                    ar.Ramo.Codigo,
                    ar.Ramo.Nome,
                    ar.NumeroApolice,
                    ar.IofPercentual,
                    ar.Ativo
                )).ToList(),
                a.Configuracao
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceRaw == null) return null;

        var nomesEstipulantes = await ObterNomesGlobaisAsync("estipulante", new List<long> { apoliceRaw.EstipulanteId }, cancellationToken);
        var nomesSeguradoras = await ObterNomesGlobaisAsync("seguradora", new List<long> { apoliceRaw.SeguradoraId }, cancellationToken);
        var nomesCorretoras = apoliceRaw.CorretoraId.HasValue 
            ? await ObterNomesGlobaisAsync("corretora", new List<long> { apoliceRaw.CorretoraId.Value }, cancellationToken) 
            : new Dictionary<long, string>();

        var apolice = new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceDetalheResult(
            apoliceRaw.PublicId,
            apoliceRaw.Nome ?? "",
            apoliceRaw.EstipulanteId,
            nomesEstipulantes.GetValueOrDefault(apoliceRaw.EstipulanteId, "Desconhecido"),
            apoliceRaw.SeguradoraId,
            nomesSeguradoras.GetValueOrDefault(apoliceRaw.SeguradoraId, "Desconhecido"),
            apoliceRaw.CorretoraId,
            apoliceRaw.CorretoraId.HasValue ? nomesCorretoras.GetValueOrDefault(apoliceRaw.CorretoraId.Value, "Desconhecido") : "Nenhuma",
            apoliceRaw.DataInicioVigencia,
            apoliceRaw.DataFimVigencia,
            apoliceRaw.DataAniversario,
            apoliceRaw.Status,
            apoliceRaw.Ativo,
            apoliceRaw.Observacao,
            apoliceRaw.Ramos,
            apoliceRaw.Configuracao != null ? new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ObterApolice.ApoliceConfiguracaoResult(
                apoliceRaw.Configuracao.TipoAdesao,
                apoliceRaw.Configuracao.Custeio,
                apoliceRaw.Configuracao.CarenciaDias,
                apoliceRaw.Configuracao.MesBaseReajuste,
                apoliceRaw.Configuracao.IndiceReajuste,
                apoliceRaw.Configuracao.CobreConjuge,
                apoliceRaw.Configuracao.ControlaExcedente,
                apoliceRaw.Configuracao.DiaCorteFaturamento,
                apoliceRaw.Configuracao.PrazoAvisoSinistroDias
            ) : null
        );

        return apolice;
    }

    public async Task<PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>> ListarVidasPaginadoAsync(
        Guid apolicePublicId,
        int pagina,
        int tamanhoPagina,
        string? buscaCliente,
        string? status,
        Guid? apoliceSubgrupoPublicId,
        Guid? apoliceModuloPublicId,
        DateOnly? vigenciaDataReferencia,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.PublicId == apolicePublicId && a.DeletedAt == null)
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

        // Resolver IDs de filtro
        long? filtroSubgrupoId = null;
        if (apoliceSubgrupoPublicId.HasValue)
        {
            var subId = await _dbContext.ApoliceSubgrupos
                .AsNoTracking()
                .Where(s => s.ApoliceId == apoliceId && s.PublicId == apoliceSubgrupoPublicId.Value && s.DeletedAt == null)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (subId == 0) return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult> { Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(), Page = pagina, PageSize = tamanhoPagina, TotalCount = 0 };
            filtroSubgrupoId = subId;
        }

        long? filtroModuloId = null;
        if (apoliceModuloPublicId.HasValue)
        {
            var modId = await _dbContext.ApoliceModulos
                .AsNoTracking()
                .Where(m => m.ApoliceId == apoliceId && m.PublicId == apoliceModuloPublicId.Value && m.DeletedAt == null)
                .Select(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (modId == 0) return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult> { Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(), Page = pagina, PageSize = tamanhoPagina, TotalCount = 0 };
            filtroModuloId = modId;
        }

        // Construir query base com filtros aplicados no banco
        var query = _dbContext.ApoliceVidas
            .AsNoTracking()
            .Where(v => v.ApoliceId == apoliceId && v.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == status);

        if (filtroSubgrupoId.HasValue)
            query = query.Where(v => v.ApoliceSubgrupoId == filtroSubgrupoId.Value);

        if (filtroModuloId.HasValue)
            query = query.Where(v => v.ApoliceModuloId == filtroModuloId.Value);

        if (vigenciaDataReferencia.HasValue)
        {
            var ref0 = vigenciaDataReferencia.Value;
            query = query.Where(v =>
                (v.DataInicioVigencia == null || v.DataInicioVigencia <= ref0) &&
                (v.DataFimVigencia == null || v.DataFimVigencia >= ref0));
        }

        var totalItens = await query.CountAsync(cancellationToken);
        var skip = (pagina - 1) * tamanhoPagina;

        var vidasRaw = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip(skip)
            .Take(tamanhoPagina)
            .Select(v => new
            {
                v.Id,
                v.PublicId,
                v.ClienteId,
                v.ApoliceSubgrupoId,
                v.ApoliceModuloId,
                v.DataInicioVigencia,
                v.DataFimVigencia,
                v.Status,
                v.Ativo,
                v.Observacao
            })
            .ToListAsync(cancellationToken);

        if (!vidasRaw.Any())
            return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>
            {
                Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(),
                Page = pagina, PageSize = tamanhoPagina, TotalCount = totalItens
            };

        // Enriquecer via ADO.NET cross-module (clientes, subestipulantes, módulos)
        var clienteIds = vidasRaw.Select(v => v.ClienteId).Distinct().ToList();
        var clientesGlobais = await ObterClientesGlobaisAsync(clienteIds, cancellationToken);

        // Filtrar por busca de cliente (após resolução do nome)
        if (!string.IsNullOrWhiteSpace(buscaCliente))
        {
            var buscaLower = buscaCliente.ToLower();
            var clientesFiltrados = clientesGlobais
                .Where(c => (c.Nome?.ToLower().Contains(buscaLower) ?? false) || (c.Documento?.Contains(buscaCliente) ?? false))
                .Select(c => c.Id)
                .ToHashSet();
            vidasRaw = vidasRaw.Where(v => clientesFiltrados.Contains(v.ClienteId)).ToList();
        }

        var subgrupoIds = vidasRaw.Where(v => v.ApoliceSubgrupoId.HasValue)
            .Select(v => v.ApoliceSubgrupoId!.Value).Distinct().ToList();
        var subgruposDict = await _dbContext.ApoliceSubgrupos
            .AsNoTracking()
            .Where(s => subgrupoIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var moduloIds = vidasRaw.Where(v => v.ApoliceModuloId.HasValue)
            .Select(v => v.ApoliceModuloId!.Value).Distinct().ToList();
        
        var apoliceModulos = await _dbContext.ApoliceModulos
            .AsNoTracking()
            .Where(m => moduloIds.Contains(m.Id))
            .ToListAsync(cancellationToken);
            
        var cadastroModuloIds = apoliceModulos.Select(m => m.ModuloId).Distinct().ToList();
        var modulosDict = await ObterModulosGlobaisAsync(cadastroModuloIds, cancellationToken);
        var apoliceModulosDict = apoliceModulos.ToDictionary(m => m.Id);

        var clientesDict = clientesGlobais.ToDictionary(c => c.Id);

        var itens = vidasRaw.Select(v =>
        {
            var cliente = clientesDict.GetValueOrDefault(v.ClienteId);
            
            var subgrupo = v.ApoliceSubgrupoId.HasValue ? subgruposDict.GetValueOrDefault(v.ApoliceSubgrupoId.Value) : null;
            
            var apoliceModulo = v.ApoliceModuloId.HasValue ? apoliceModulosDict.GetValueOrDefault(v.ApoliceModuloId.Value) : null;
            var moduloGlobal = apoliceModulo != null ? modulosDict.GetValueOrDefault(apoliceModulo.ModuloId) : null;

            var docMascarado = MascararDocumento(cliente?.Documento);

            return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult(
                v.PublicId,
                cliente?.PublicId ?? Guid.Empty,
                cliente?.Nome ?? $"Cliente {v.ClienteId}",
                docMascarado,
                subgrupo?.PublicId,
                subgrupo?.Nome,
                apoliceModulo?.PublicId,
                moduloGlobal?.ModuloNome,
                v.DataInicioVigencia,
                v.DataFimVigencia,
                v.Status,
                v.Ativo,
                v.Observacao
            );
        }).ToList();

        return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>
        {
            Items = itens,
            Page = pagina,
            PageSize = tamanhoPagina,
            TotalCount = totalItens
        };
    }

    public async Task<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult?> ObterApoliceVidaPorPublicIdAsync(
        Guid apolicePublicId,
        Guid apoliceVidaPublicId,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.PublicId == apolicePublicId && a.DeletedAt == null)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0) return null;

        var v = await _dbContext.ApoliceVidas
            .AsNoTracking()
            .Where(x => x.PublicId == apoliceVidaPublicId && x.ApoliceId == apoliceId && x.DeletedAt == null)
            .Select(x => new
            {
                x.PublicId,
                x.ClienteId,
                x.ApoliceSubgrupoId,
                x.ApoliceModuloId,
                x.DataInicioVigencia,
                x.DataFimVigencia,
                x.Status,
                x.Ativo,
                x.Observacao
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (v == null) return null;

        var clientesGlobais = await ObterClientesGlobaisAsync(new List<long> { v.ClienteId }, cancellationToken);
        var cliente = clientesGlobais.FirstOrDefault();

        ApoliceSubgrupoModel? subgrupo = null;
        if (v.ApoliceSubgrupoId.HasValue)
        {
            subgrupo = await _dbContext.ApoliceSubgrupos
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == v.ApoliceSubgrupoId.Value, cancellationToken);
        }

        ApoliceModuloModel? apoliceModulo = null;
        VidaModuloQueryDto? moduloGlobal = null;
        if (v.ApoliceModuloId.HasValue)
        {
            apoliceModulo = await _dbContext.ApoliceModulos
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == v.ApoliceModuloId.Value, cancellationToken);
                
            if (apoliceModulo != null)
            {
                var modDict = await ObterModulosGlobaisAsync(new List<long> { apoliceModulo.ModuloId }, cancellationToken);
                moduloGlobal = modDict.GetValueOrDefault(apoliceModulo.ModuloId);
            }
        }

        return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult(
            v.PublicId,
            cliente?.PublicId ?? Guid.Empty,
            cliente?.Nome ?? $"Cliente {v.ClienteId}",
            MascararDocumento(cliente?.Documento),
            subgrupo?.PublicId,
            subgrupo?.Nome,
            apoliceModulo?.PublicId,
            moduloGlobal?.ModuloNome,
            v.DataInicioVigencia,
            v.DataFimVigencia,
            v.Status,
            v.Ativo,
            v.Observacao
        );
    }

    public class SubestipulanteGlobalDto
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string Nome { get; set; } = null!;
        public string? Documento { get; set; }
        public string? Codigo { get; set; }
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

        var vinculos = await _dbContext.ApoliceSubestipulantes
            .AsNoTracking()
            .Where(s => s.ApoliceId == apoliceId && s.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (!vinculos.Any())
            return new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult>();

        var subIds = vinculos.Select(v => v.SubestipulanteId).Distinct().ToList();
        var subsCsv = string.Join(",", subIds);

        // Usa ADO.NET direto para evitar conflito de snake_case convention do EF Core
        var subestipulantesGlobais = new List<SubestipulanteGlobalDto>();
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT s.id, s.public_id, p.nome, p.documento_principal, s.codigo FROM cadastro.subestipulante s INNER JOIN core.pessoa p ON s.pessoa_id = p.id WHERE s.id IN ({subsCsv})";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                subestipulantesGlobais.Add(new SubestipulanteGlobalDto
                {
                    Id = reader.GetInt64(0),
                    PublicId = reader.GetGuid(1),
                    Nome = reader.GetString(2),
                    Documento = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Codigo = reader.IsDBNull(4) ? null : reader.GetString(4),
                });
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        var subestipulantesDict = subestipulantesGlobais.ToDictionary(s => s.Id);



        var itens = vinculos.Select(s =>
        {
            var subGlobal = subestipulantesDict.GetValueOrDefault(s.SubestipulanteId);

            return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult(
                subGlobal?.PublicId ?? Guid.Empty,
                subGlobal?.Nome ?? "Desconhecido",
                subGlobal?.Documento,
                subGlobal?.Codigo,
                s.DataInicio,
                s.DataFim,
                s.Ativo
            );
        }).ToList();

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



    private class ModuloGlobalQueryDto
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }
        public bool Ativo { get; set; }
    }

    // ─── Helpers cross-module para ListarVidas e ObterApoliceVida ───────────────

    private sealed class ClienteGlobalQueryDto
    {
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string Nome { get; set; } = null!;
        public string? Documento { get; set; }
    }


    private sealed class VidaModuloQueryDto
    {
        public long ModuloId { get; set; }
        public Guid ModuloPublicId { get; set; }
        public string ModuloNome { get; set; } = null!;
    }

    private async Task<List<ClienteGlobalQueryDto>> ObterClientesGlobaisAsync(List<long> clienteIds, CancellationToken cancellationToken)
    {
        var result = new List<ClienteGlobalQueryDto>();
        if (!clienteIds.Any()) return result;

        var idsCsv = string.Join(",", clienteIds);
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT c.id, c.public_id, p.nome, p.documento_principal FROM cadastro.cliente c INNER JOIN core.pessoa p ON c.pessoa_id = p.id WHERE c.id IN ({idsCsv}) AND c.deleted_at IS NULL";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new ClienteGlobalQueryDto
                {
                    Id = reader.GetInt64(0),
                    PublicId = reader.GetGuid(1),
                    Nome = reader.GetString(2),
                    Documento = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
        return result;
    }



    /// <summary>
    /// Retorna dados do Módulo Global.
    /// Key do dicionário = cadastro.modulo.id.
    /// </summary>
    private async Task<Dictionary<long, VidaModuloQueryDto>> ObterModulosGlobaisAsync(List<long> cadastroModuloIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, VidaModuloQueryDto>();
        if (!cadastroModuloIds.Any()) return result;

        var idsCsv = string.Join(",", cadastroModuloIds);
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT m.id, m.public_id, m.nome
                FROM cadastro.modulo m
                WHERE m.id IN ({idsCsv})";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var dto = new VidaModuloQueryDto
                {
                    ModuloId = reader.GetInt64(0),
                    ModuloPublicId = reader.GetGuid(1),
                    ModuloNome = reader.GetString(2)
                };
                result[dto.ModuloId] = dto;
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
        return result;
    }

    private static string MascararDocumento(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento)) return "";
        // CPF: remove não-dígitos e mascara como ***.***.000-**
        var digits = System.Text.RegularExpressions.Regex.Replace(documento, @"\D", "");
        if (digits.Length == 11)
            return $"***.***.{digits.Substring(6, 3)}-**";
        if (digits.Length == 14)
            return $"**.***.{digits.Substring(5, 3)}/{digits.Substring(8, 4)}-**";
        return "***";
    }

    // ── Subgrupos da Apólice ─────────────────────────────────────────────────

    /// <summary>
    /// Lista todos os Subgrupos de uma Apólice.
    /// Subgrupo é uma divisão contextual — não é cadastro global.
    /// </summary>
    public async Task<List<ApoliceSubgrupoResult>> ListarSubgruposAsync(
        Guid apolicePublicId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ApoliceSubgrupos
            .AsNoTracking()
            .Where(s => s.Apolice!.PublicId == apolicePublicId
                     && s.DeletedAt == null)
            .OrderBy(s => s.Nome)
            .Select(s => new ApoliceSubgrupoResult(
                s.PublicId,
                s.Nome,
                s.Observacao,
                s.Ativo))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém um Subgrupo específico dentro de uma Apólice pelo PublicId.
    /// Valida isolamento: retorna null se o subgrupo não pertencer à apólice indicada.
    /// </summary>
    public async Task<ApoliceSubgrupoResult?> ObterSubgrupoPorPublicIdAsync(
        Guid apolicePublicId,
        Guid subgrupoPublicId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ApoliceSubgrupos
            .AsNoTracking()
            .Where(s => s.PublicId == subgrupoPublicId
                     && s.Apolice!.PublicId == apolicePublicId
                     && s.DeletedAt == null)
            .Select(s => new ApoliceSubgrupoResult(
                s.PublicId,
                s.Nome,
                s.Observacao,
                s.Ativo))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Dictionary<long, string>> ObterNomesGlobaisAsync(string tabela, List<long> ids, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, string>();
        if (!ids.Any()) return result;

        var idsCsv = string.Join(",", ids.Distinct());
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT e.id, p.nome 
                FROM cadastro.{tabela} e 
                INNER JOIN core.pessoa p ON e.pessoa_id = p.id 
                WHERE e.id IN ({idsCsv})";
            
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result[reader.GetInt64(0)] = reader.GetString(1);
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
        return result;
    }
}
