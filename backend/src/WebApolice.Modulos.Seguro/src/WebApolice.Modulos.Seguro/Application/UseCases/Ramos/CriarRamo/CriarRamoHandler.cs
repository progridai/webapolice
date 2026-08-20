using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Seguro.Application.DTOs;
using WebApolice.Modulos.Seguro.Application.Mappers;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Ramos.CriarRamo;

public class CriarRamoHandler
{
    private readonly SeguroDbContext _context;

    public CriarRamoHandler(SeguroDbContext context)
    {
        _context = context;
    }

    public async Task<RamoDto> Handle(CriarRamoCommand command, CancellationToken cancellationToken)
    {
        var model = new RamoModel
        {
            PublicId = Guid.NewGuid(),
            Codigo = command.Codigo,
            Nome = command.Nome,
            Descricao = command.Descricao,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Ramos.Add(model);
        await _context.SaveChangesAsync(cancellationToken);

        return model.ToDto();
    }
}
