using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Perfis;

public class ObterPerfilUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ObterPerfilUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PerfilDetalheDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var perfil = await _dbContext.Perfis
            .Include(p => p.Permissoes)
            .ThenInclude(pp => pp.Permissao)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PublicId == publicId, cancellationToken);

        if (perfil == null) return null;

        var permissoesIds = perfil.Permissoes.Select(pp => pp.Permissao.PublicId).ToList();

        return new PerfilDetalheDto(
            perfil.PublicId,
            perfil.Codigo,
            perfil.Nome,
            perfil.Descricao,
            perfil.Ativo,
            perfil.PerfilSistema,
            perfil.AcessoTotal,
            permissoesIds
        );
    }
}
