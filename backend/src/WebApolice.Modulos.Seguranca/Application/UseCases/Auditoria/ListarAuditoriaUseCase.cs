using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria;

public class ListarAuditoriaUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarAuditoriaUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListagemPaginadaDto<AuditoriaDto>> ExecuteAsync(
        int pagina,
        int tamanhoPagina,
        string? acao,
        string? entidade,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditoriaPermissoes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(acao))
        {
            query = query.Where(a => a.Acao.Contains(acao));
        }
        
        if (!string.IsNullOrWhiteSpace(entidade))
        {
            query = query.Where(a => a.EntidadeTipo.Contains(entidade));
        }

        if (dataInicial.HasValue)
        {
            var start = dataInicial.Value.Date.ToUniversalTime();
            query = query.Where(a => a.CreatedAt >= start);
        }

        if (dataFinal.HasValue)
        {
            var end = dataFinal.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(a => a.CreatedAt <= end);
        }

        var totalItens = await query.CountAsync(cancellationToken);
        
        pagina = pagina > 0 ? pagina : 1;
        tamanhoPagina = tamanhoPagina > 0 ? tamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;
        
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var auditorias = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        var itens = auditorias.Select(a => new AuditoriaDto(
            a.PublicId,
            a.Acao,
            a.EntidadeTipo,
            a.EntidadeId.ToString(),
            a.CreatedAt,
            a.DadosAnteriores,
            a.DadosNovos
        )).ToList();

        return new ListagemPaginadaDto<AuditoriaDto>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
}
