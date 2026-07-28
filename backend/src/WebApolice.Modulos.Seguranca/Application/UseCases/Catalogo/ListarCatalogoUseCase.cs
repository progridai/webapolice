using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Catalogo;

public class ListarCatalogoUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ListarCatalogoUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CatalogoModuloDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var modulos = await _dbContext.Modulos
            .Include(m => m.Recursos)
            .ThenInclude(r => r.Permissoes)
            .AsNoTracking()
            .OrderBy(m => m.Ordem)
            .ToListAsync(cancellationToken);

        return modulos.Select(m => new CatalogoModuloDto(
            m.PublicId,
            m.Codigo,
            m.Nome,
            m.Descricao,
            m.Icone,
            m.Recursos.OrderBy(r => r.Ordem).Select(r => new CatalogoRecursoDto(
                r.PublicId,
                r.Codigo,
                r.Nome,
                r.Descricao,
                r.RotaFrontend,
                r.Permissoes.OrderBy(p => p.Nome).Select(p => new CatalogoPermissaoDto(
                    p.PublicId,
                    p.Codigo,
                    p.Nome,
                    p.Descricao
                )).ToList()
            )).ToList()
        )).ToList();
    }
}
