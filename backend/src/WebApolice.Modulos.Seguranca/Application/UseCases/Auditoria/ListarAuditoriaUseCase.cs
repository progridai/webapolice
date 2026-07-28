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
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditoriaPermissoes.AsNoTracking();

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
