using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;
using System.Linq;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.AtualizarSubestipulante;

public class AtualizarSubestipulanteApoliceHandler : IRequestHandler<AtualizarSubestipulanteApoliceCommand>
{
    private readonly SeguroDbContext _dbContext;

    public AtualizarSubestipulanteApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AtualizarSubestipulanteApoliceCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser menor que a data de início.");
        }

        var apolice = await _dbContext.Apolices.FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        var subestipulanteId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId}")
            .FirstOrDefaultAsync(cancellationToken);

        if (subestipulanteId == 0)
        {
            throw new ValidacaoException("Subestipulante não encontrado.");
        }

        var vinculoExistente = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(ar => ar.ApoliceId == apolice.Id && ar.SubestipulanteId == subestipulanteId, cancellationToken);
        
        if (vinculoExistente == null)
        {
            throw new ValidacaoException("Vínculo não encontrado.");
        }

        if (!vinculoExistente.Ativo)
        {
            throw new ValidacaoException("Não é possível alterar um vínculo inativo.");
        }

        vinculoExistente.DataInicio = request.DataInicio;
        vinculoExistente.DataFim = request.DataFim;
        vinculoExistente.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApoliceSubestipulantes.Update(vinculoExistente);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
