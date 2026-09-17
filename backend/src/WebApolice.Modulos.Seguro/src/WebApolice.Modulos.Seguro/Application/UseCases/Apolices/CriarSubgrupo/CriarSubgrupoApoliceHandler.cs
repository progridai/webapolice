using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.Application.UseCases.Apolices.CriarSubgrupo;

public class CriarSubgrupoApoliceHandler
{
    private readonly SeguroDbContext _dbContext;

    public CriarSubgrupoApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CriarSubgrupoApoliceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            throw new ValidacaoException("O nome do Subgrupo é obrigatório.");

        if (request.Nome.Length > 200)
            throw new ValidacaoException("O nome do Subgrupo não pode exceder 200 caracteres.");

        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);

        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var subgrupo = new ApoliceSubgrupoModel
        {
            ApoliceId = apolice.Id,
            Nome = request.Nome.Trim(),
            Observacao = request.Observacao?.Trim(),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ApoliceSubgrupos.Add(subgrupo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return subgrupo.PublicId;
    }
}
