using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class ListarPerfisUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarPerfisUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListagemPaginadaDto<PerfilDto>> ExecuteAsync(
        string? busca,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Perfis.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            busca = busca.ToLower();
            query = query.Where(p => p.Nome.ToLower().Contains(busca) || p.Codigo.ToLower().Contains(busca));
        }

        if (ativo.HasValue)
        {
            query = query.Where(p => p.Ativo == ativo.Value);
        }

        var totalItens = await query.CountAsync(cancellationToken);
        
        pagina = pagina > 0 ? pagina : 1;
        tamanhoPagina = tamanhoPagina > 0 ? tamanhoPagina : 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;
        
        var totalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);

        var perfis = await query
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        var itens = perfis.Select(p => new PerfilDto(
            p.PublicId,
            p.Codigo,
            p.Nome,
            p.Descricao,
            p.Ativo,
            p.PerfilSistema,
            p.AcessoTotal
        )).ToList();

        return new ListagemPaginadaDto<PerfilDto>(itens, pagina, tamanhoPagina, totalItens, totalPaginas);
    }
}
