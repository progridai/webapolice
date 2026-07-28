using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Modulos;

public record ModuloDto(Guid PublicId, string Codigo, string Nome, string Descricao, string Icone, int Ordem, bool Ativo, bool Habilitado);

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
            .OrderBy(m => m.Ordem)
            .Select(m => new ModuloDto(m.PublicId, m.Codigo, m.Nome, m.Descricao, m.Icone, m.Ordem, m.Ativo, m.Habilitado))
            .ToListAsync(cancellationToken);
    }
}
