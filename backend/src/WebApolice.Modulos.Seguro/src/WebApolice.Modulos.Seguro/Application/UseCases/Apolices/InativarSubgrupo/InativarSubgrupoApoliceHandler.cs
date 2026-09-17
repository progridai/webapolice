using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubgrupo;

public class InativarSubgrupoApoliceHandler
{
    private readonly SeguroDbContext _dbContext;

    public InativarSubgrupoApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(InativarSubgrupoApoliceCommand request, CancellationToken cancellationToken)
    {
        // Resolve a Apólice para garantir isolamento
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var subgrupo = await _dbContext.ApoliceSubgrupos
            .FirstOrDefaultAsync(
                s => s.PublicId == request.SubgrupoPublicId
                  && s.ApoliceId == apolice.Id
                  && s.DeletedAt == null,
                cancellationToken);

        if (subgrupo == null)
            throw new ValidacaoException("Subgrupo não encontrado nesta Apólice.");

        if (!subgrupo.Ativo)
            throw new ValidacaoException("O Subgrupo já está inativo.");

        subgrupo.Ativo = false;
        subgrupo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApoliceSubgrupos.Update(subgrupo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
