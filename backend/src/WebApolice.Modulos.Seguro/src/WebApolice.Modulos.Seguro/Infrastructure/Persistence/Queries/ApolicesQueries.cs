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
                // apolice_subestipulante_modulo_id: coluna existente no model e mapeada no EF
                // A projeção do public_id do módulo requer JOIN cross-schema — retornado como null neste contexto de listagem geral
                null,
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
}
