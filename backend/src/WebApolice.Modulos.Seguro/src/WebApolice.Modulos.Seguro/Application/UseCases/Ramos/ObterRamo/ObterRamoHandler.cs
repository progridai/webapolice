using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.ObterRamo;

public class ObterRamoHandler
{
    private readonly SeguroDbContext _context;

    public ObterRamoHandler(SeguroDbContext context)
    {
        _context = context;
    }

    public async Task<RamoDto?> Handle(ObterRamoQuery query, CancellationToken cancellationToken)
    {
        var entity = await _context.Ramos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PublicId == query.PublicId, cancellationToken);

        return entity?.ToDto();
    }
}
