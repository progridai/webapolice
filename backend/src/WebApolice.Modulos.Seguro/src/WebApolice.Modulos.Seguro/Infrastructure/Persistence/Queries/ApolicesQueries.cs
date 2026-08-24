using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarApolices;
using WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarModulos;
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
        string? buscaCliente,
        string? status,
        Guid? subestipulantePublicId,
        Guid? moduloPublicId,
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

        // Resolver IDs de filtro cross-module (antes de aplicar no EF)
        long? filtroSubestipulanteId = null;
        if (subestipulantePublicId.HasValue)
        {
            var subId = await _dbContext.Database
                .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {subestipulantePublicId.Value} AND deleted_at IS NULL")
                .FirstOrDefaultAsync(cancellationToken);
            if (subId == 0) return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult> { Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(), Page = pagina, PageSize = tamanhoPagina, TotalCount = 0 };

            // Resolver o apolice_subestipulante_id (FK local) a partir do subestipulante global
            var vinculoSub = await _dbContext.ApoliceSubestipulantes
                .AsNoTracking()
                .Where(s => s.ApoliceId == apoliceId && s.SubestipulanteId == subId && s.DeletedAt == null)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (vinculoSub == 0) return new PagedResult<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult> { Items = new List<WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult>(), Page = pagina, PageSize = tamanhoPagina, TotalCount = 0 };
            filtroSubestipulanteId = vinculoSub;
        }

        long? filtroModuloVinculoId = null;
        if (moduloPublicId.HasValue && filtroSubestipulanteId.HasValue)
        {
            var modId = await _dbContext.Database
                .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.modulo WHERE public_id = {moduloPublicId.Value} AND deleted_at IS NULL")
                .FirstOrDefaultAsync(cancellationToken);
            if (modId > 0)
            {
                var vinculoMod = await _dbContext.ApoliceSubestipulanteModulos
                    .AsNoTracking()
                    .Where(m => m.ApoliceSubestipulanteId == filtroSubestipulanteId.Value && m.ModuloId == modId && m.DeletedAt == null)
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                filtroModuloVinculoId = vinculoMod > 0 ? vinculoMod : null;
            }
        }

        // Construir query base com filtros aplicados no banco
        var query = _dbContext.ApoliceVidas
            .AsNoTracking()
            .Where(v => v.ApoliceId == apoliceId && v.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == status);

        if (filtroSubestipulanteId.HasValue)
            query = query.Where(v => v.ApoliceSubestipulanteId == filtroSubestipulanteId.Value);

        if (filtroModuloVinculoId.HasValue)
            query = query.Where(v => v.ApoliceSubestipulanteModuloId == filtroModuloVinculoId.Value);

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
                v.ApoliceSubestipulanteId,
                v.ApoliceSubestipulanteModuloId,
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

        var subVinculoIds = vidasRaw.Where(v => v.ApoliceSubestipulanteId.HasValue)
            .Select(v => v.ApoliceSubestipulanteId!.Value).Distinct().ToList();
        var subGlobaisDict = await ObterSubestipulantesGlobaisAsync(subVinculoIds, cancellationToken);

        var moduloVinculoIds = vidasRaw.Where(v => v.ApoliceSubestipulanteModuloId.HasValue)
            .Select(v => v.ApoliceSubestipulanteModuloId!.Value).Distinct().ToList();
        var modulosDict = await ObterModulosGlobaisAsync(moduloVinculoIds, cancellationToken);

        var clientesDict = clientesGlobais.ToDictionary(c => c.Id);

        var itens = vidasRaw.Select(v =>
        {
            var cliente = clientesDict.GetValueOrDefault(v.ClienteId);
            var subVinculo = v.ApoliceSubestipulanteId.HasValue ? subGlobaisDict.GetValueOrDefault(v.ApoliceSubestipulanteId.Value) : null;
            var moduloVinculo = v.ApoliceSubestipulanteModuloId.HasValue ? modulosDict.GetValueOrDefault(v.ApoliceSubestipulanteModuloId.Value) : null;

            var contexto = v.ApoliceSubestipulanteModuloId.HasValue ? "modulo"
                : v.ApoliceSubestipulanteId.HasValue ? "subestipulante"
                : "direto";

            var docMascarado = MascararDocumento(cliente?.Documento);

            return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult(
                v.PublicId,
                cliente?.PublicId ?? Guid.Empty,
                cliente?.Nome ?? $"Cliente {v.ClienteId}",
                docMascarado,
                contexto,
                subVinculo?.SubestipulantePublicId,
                subVinculo?.SubestipulanteNome,
                moduloVinculo?.ModuloPublicId,
                moduloVinculo?.ModuloNome,
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
                x.ApoliceSubestipulanteId,
                x.ApoliceSubestipulanteModuloId,
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

        VidaSubestipulanteQueryDto? subVinculo = null;
        if (v.ApoliceSubestipulanteId.HasValue)
        {
            var subDict = await ObterSubestipulantesGlobaisAsync(new List<long> { v.ApoliceSubestipulanteId.Value }, cancellationToken);
            subVinculo = subDict.GetValueOrDefault(v.ApoliceSubestipulanteId.Value);
        }

        VidaModuloQueryDto? moduloVinculo = null;
        if (v.ApoliceSubestipulanteModuloId.HasValue)
        {
            var modDict = await ObterModulosGlobaisAsync(new List<long> { v.ApoliceSubestipulanteModuloId.Value }, cancellationToken);
            moduloVinculo = modDict.GetValueOrDefault(v.ApoliceSubestipulanteModuloId.Value);
        }

        var contexto = v.ApoliceSubestipulanteModuloId.HasValue ? "modulo"
            : v.ApoliceSubestipulanteId.HasValue ? "subestipulante"
            : "direto";

        return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarVidas.ApoliceVidaResult(
            v.PublicId,
            cliente?.PublicId ?? Guid.Empty,
            cliente?.Nome ?? $"Cliente {v.ClienteId}",
            MascararDocumento(cliente?.Documento),
            contexto,
            subVinculo?.SubestipulantePublicId,
            subVinculo?.SubestipulanteNome,
            moduloVinculo?.ModuloPublicId,
            moduloVinculo?.ModuloNome,
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

        // Carregar módulos vinculados para cada subestipulante (sem N+1)
        var vinculoIds = vinculos.Select(v => v.Id).ToList();
        var moduloVinculos = await _dbContext.ApoliceSubestipulanteModulos
            .AsNoTracking()
            .Where(m => vinculoIds.Contains(m.ApoliceSubestipulanteId) && m.DeletedAt == null)
            .ToListAsync();

        // Carregar dados globais dos módulos via ADO.NET (cross-schema cadastro → seguro)
        var moduloIds = moduloVinculos.Select(m => m.ModuloId).Distinct().ToList();
        var modulosGlobais = new List<ModuloGlobalQueryDto>();
        if (moduloIds.Any())
        {
            var idsCsv = string.Join(",", moduloIds);
            var connModulo = _dbContext.Database.GetDbConnection();
            var wasOpenModulo = connModulo.State == System.Data.ConnectionState.Open;
            if (!wasOpenModulo) await connModulo.OpenAsync();
            try
            {
                using var cmd = connModulo.CreateCommand();
                cmd.CommandText = $"SELECT id, public_id, nome, descricao, ativo FROM cadastro.modulo WHERE id IN ({idsCsv})";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    modulosGlobais.Add(new ModuloGlobalQueryDto
                    {
                        Id = reader.GetInt64(0),
                        PublicId = reader.GetGuid(1),
                        Nome = reader.GetString(2),
                        Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Ativo = reader.GetBoolean(4)
                    });
                }
            }
            finally
            {
                if (!wasOpenModulo) await connModulo.CloseAsync();
            }
        }
        var modulosGlobaisDict = modulosGlobais.ToDictionary(m => m.Id);

        var itens = vinculos.Select(s =>
        {
            var subGlobal = subestipulantesDict.GetValueOrDefault(s.SubestipulanteId);
            var modulosDoSub = moduloVinculos
                .Where(m => m.ApoliceSubestipulanteId == s.Id)
                .Select(m =>
                {
                    var mg = modulosGlobaisDict.GetValueOrDefault(m.ModuloId);
                    return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteModuloResult(
                        mg?.PublicId ?? Guid.Empty,
                        mg?.Nome ?? "Desconhecido",
                        mg?.Descricao,
                        mg?.Ativo ?? false,
                        m.Ativo,
                        m.DataInicio,
                        m.DataFim
                    );
                }).ToList();

            return new WebApolice.Modulos.Seguro.Application.UseCases.Apolices.ListarSubestipulantes.ApoliceSubestipulanteResult(
                subGlobal?.PublicId ?? Guid.Empty,
                subGlobal?.Nome ?? "Desconhecido",
                subGlobal?.Documento,
                subGlobal?.Codigo,
                s.DataInicio,
                s.DataFim,
                s.Ativo,
                modulosDoSub
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

    /// <summary>
    /// Lista os Módulos vinculados a um Subestipulante no contexto de uma Apólice.
    /// Usa ADO.NET cross-schema (padrão vigente) para resolver dados do Catálogo Global (cadastro.modulo).
    /// AsNoTracking + select eficiente — sem N+1.
    /// </summary>
    public async Task<List<ModuloDoSubestipulanteResult>> ListarModulosDoSubestipulanteAsync(
        Guid apolicePublicId,
        Guid subestipulantePublicId,
        CancellationToken cancellationToken)
    {
        var apoliceId = await _dbContext.Apolices
            .AsNoTracking()
            .Where(a => a.PublicId == apolicePublicId && a.DeletedAt == null)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (apoliceId == 0)
            return new List<ModuloDoSubestipulanteResult>();

        // Resolver subestipulanteId cross-module
        var subestipulanteId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {subestipulantePublicId} AND deleted_at IS NULL")
            .FirstOrDefaultAsync(cancellationToken);

        if (subestipulanteId == 0)
            return new List<ModuloDoSubestipulanteResult>();

        // Localizar vínculo pai
        var vinculoPaiId = await _dbContext.ApoliceSubestipulantes
            .AsNoTracking()
            .Where(s => s.ApoliceId == apoliceId && s.SubestipulanteId == subestipulanteId && s.DeletedAt == null)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (vinculoPaiId == 0)
            return new List<ModuloDoSubestipulanteResult>();

        // Carregar todos os vínculos do subestipulante (sem filtro de ativo — mostra histórico também)
        var vinculos = await _dbContext.ApoliceSubestipulanteModulos
            .AsNoTracking()
            .Where(m => m.ApoliceSubestipulanteId == vinculoPaiId && m.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (!vinculos.Any())
            return new List<ModuloDoSubestipulanteResult>();

        // Carregar dados globais via ADO.NET (cross-schema — padrão vigente)
        var moduloIds = vinculos.Select(m => m.ModuloId).Distinct().ToList();
        var idsCsv = string.Join(",", moduloIds);
        var modulosGlobais = new List<ModuloGlobalQueryDto>();

        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT id, public_id, nome, descricao, ativo FROM cadastro.modulo WHERE id IN ({idsCsv})";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                modulosGlobais.Add(new ModuloGlobalQueryDto
                {
                    Id = reader.GetInt64(0),
                    PublicId = reader.GetGuid(1),
                    Nome = reader.GetString(2),
                    Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Ativo = reader.GetBoolean(4)
                });
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }

        var modulosDict = modulosGlobais.ToDictionary(m => m.Id);

        return vinculos.Select(m =>
        {
            var mg = modulosDict.GetValueOrDefault(m.ModuloId);
            return new ModuloDoSubestipulanteResult(
                mg?.PublicId ?? Guid.Empty,
                mg?.Nome ?? "Desconhecido",
                mg?.Descricao,
                mg?.Ativo ?? false,
                m.Ativo,
                m.DataInicio,
                m.DataFim
            );
        }).ToList();
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

    private sealed class VidaSubestipulanteQueryDto
    {
        public long ApoliceSubestipulanteId { get; set; }
        public Guid SubestipulantePublicId { get; set; }
        public string SubestipulanteNome { get; set; } = null!;
    }

    private sealed class VidaModuloQueryDto
    {
        public long ApoliceSubestipulanteModuloId { get; set; }
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
    /// Retorna dados do Subestipulante Global para cada apolice_subestipulante.id (FK local).
    /// Key do dicionário = apolice_subestipulante.id.
    /// </summary>
    private async Task<Dictionary<long, VidaSubestipulanteQueryDto>> ObterSubestipulantesGlobaisAsync(List<long> apoliceSubIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, VidaSubestipulanteQueryDto>();
        if (!apoliceSubIds.Any()) return result;

        var idsCsv = string.Join(",", apoliceSubIds);
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            // Junta apolice_subestipulante (seguro) com cadastro.subestipulante e core.pessoa
            cmd.CommandText = $@"
                SELECT aps.id, s.public_id, p.nome
                FROM seguro.apolice_subestipulante aps
                INNER JOIN cadastro.subestipulante s ON s.id = aps.subestipulante_id
                INNER JOIN core.pessoa p ON p.id = s.pessoa_id
                WHERE aps.id IN ({idsCsv})";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var dto = new VidaSubestipulanteQueryDto
                {
                    ApoliceSubestipulanteId = reader.GetInt64(0),
                    SubestipulantePublicId = reader.GetGuid(1),
                    SubestipulanteNome = reader.GetString(2)
                };
                result[dto.ApoliceSubestipulanteId] = dto;
            }
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
        return result;
    }

    /// <summary>
    /// Retorna dados do Módulo Global para cada apolice_subestipulante_modulo.id (FK local).
    /// Key do dicionário = apolice_subestipulante_modulo.id.
    /// </summary>
    private async Task<Dictionary<long, VidaModuloQueryDto>> ObterModulosGlobaisAsync(List<long> apoliceModuloIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<long, VidaModuloQueryDto>();
        if (!apoliceModuloIds.Any()) return result;

        var idsCsv = string.Join(",", apoliceModuloIds);
        var conn = _dbContext.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(cancellationToken);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT apm.id, m.public_id, m.nome
                FROM seguro.apolice_subestipulante_modulo apm
                INNER JOIN cadastro.modulo m ON m.id = apm.modulo_id
                WHERE apm.id IN ({idsCsv})";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var dto = new VidaModuloQueryDto
                {
                    ApoliceSubestipulanteModuloId = reader.GetInt64(0),
                    ModuloPublicId = reader.GetGuid(1),
                    ModuloNome = reader.GetString(2)
                };
                result[dto.ApoliceSubestipulanteModuloId] = dto;
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
}
