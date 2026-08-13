using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

public record RecursoDto(Guid PublicId, string Codigo, string Nome, bool Ativo, bool Habilitado);
public record ModuloDto(Guid PublicId, string Codigo, string Nome, string Descricao, string Icone, int Ordem, bool Ativo, bool Habilitado, List<RecursoDto> Recursos);

public class ListarModulosUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarModulosUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ModuloDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Modulos
            .Include(m => m.Recursos)
            .OrderBy(m => m.Ordem)
            .Select(m => new ModuloDto(
                m.PublicId, m.Codigo, m.Nome, m.Descricao, m.Icone, m.Ordem, m.Ativo, m.Habilitado,
                m.Recursos.Select(r => new RecursoDto(r.PublicId, r.Codigo, r.Nome, r.Ativo, r.Habilitado)).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}
