using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguranca.Application.DTOs;
using WebApolice.Modulos.Seguranca.Infrastructure.Persistence;

namespace WebApolice.Modulos.Seguranca.Application.UseCases.Auditoria;

public class ObterAuditoriaUseCase
{
    private readonly SegurancaDbContext _dbContext;

    public ObterAuditoriaUseCase(SegurancaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditoriaDto?> ExecuteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var auditoria = await _dbContext.AuditoriaPermissoes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);

        if (auditoria == null) return null;

        return new AuditoriaDto(
            auditoria.PublicId,
            auditoria.Acao,
            auditoria.EntidadeTipo,
            auditoria.EntidadeId.ToString(),
            auditoria.CreatedAt,
            auditoria.DadosAnteriores,
            auditoria.DadosNovos
        );
    }
}
