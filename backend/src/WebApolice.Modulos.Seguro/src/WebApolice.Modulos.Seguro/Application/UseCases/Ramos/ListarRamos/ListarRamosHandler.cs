using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.SharedKernel.Application.Models;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ListarRamos;

public class ListarRamosHandler
{
    private readonly SeguroDbContext _context;

    public ListarRamosHandler(SeguroDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RamoDto>> Handle(ListarRamosQuery query, CancellationToken cancellationToken)
    {
        var dbQuery = _context.Ramos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Busca))
        {
            var busca = query.Busca.Trim().ToLower();
            dbQuery = dbQuery.Where(x => 
                EF.Functions.ILike(x.Nome, $"%{busca}%") || 
                EF.Functions.ILike(x.Codigo, $"%{busca}%"));
        }

        if (query.Ativo.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.Ativo == query.Ativo.Value);
        }

        var total = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderBy(x => x.Nome)
            .Skip((query.Pagina - 1) * query.TamanhoPagina)
            .Take(query.TamanhoPagina)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<RamoDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Pagina,
            PageSize = query.TamanhoPagina
        };
    }
}
