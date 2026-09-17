using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AlterarSubgrupo;

public class AlterarSubgrupoApoliceHandler
{
    private readonly SeguroDbContext _dbContext;

    public AlterarSubgrupoApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AlterarSubgrupoApoliceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new ValidacaoException("O nome do Subgrupo é obrigatório.");

        if (request.Nome.Length > 200)
            throw new ValidacaoException("O nome do Subgrupo não pode exceder 200 caracteres.");

        // Resolve a Apólice para garantir isolamento: o subgrupo deve pertencer a esta Apólice
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

        subgrupo.Nome = request.Nome.Trim();
        subgrupo.Observacao = request.Observacao?.Trim();
        subgrupo.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApoliceSubgrupos.Update(subgrupo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
