using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo;

public class ListarPermissoesUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarPermissoesUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CatalogoPermissaoDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var permissoes = await _dbContext.Permissoes
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return permissoes.Select(p => new CatalogoPermissaoDto(
            p.PublicId,
            p.Codigo,
            p.Nome,
            p.Descricao
        )).ToList();
    }
}
