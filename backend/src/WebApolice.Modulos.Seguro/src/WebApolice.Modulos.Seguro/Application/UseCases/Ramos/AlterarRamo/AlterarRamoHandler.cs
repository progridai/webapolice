using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.SharedKernel.Application.Exceptions;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.AlterarRamo;

public class AlterarRamoHandler
{
    private readonly SeguroDbContext _context;

    public AlterarRamoHandler(SeguroDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AlterarRamoCommand command, CancellationToken cancellationToken)
    {
        var model = await _context.Ramos
            .FirstOrDefaultAsync(x => x.PublicId == command.PublicId, cancellationToken);

        if (model == null)
            throw new InvalidOperationException("Ramo não encontrado.");

        model.Nome = command.Nome;
        model.Descricao = command.Descricao;
        model.UpdatedAt = DateTimeOffset.UtcNow;

        _context.Ramos.Update(model);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
