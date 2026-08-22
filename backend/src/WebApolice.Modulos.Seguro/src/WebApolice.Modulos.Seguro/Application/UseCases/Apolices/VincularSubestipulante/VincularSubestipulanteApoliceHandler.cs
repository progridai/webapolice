using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;
using System.Linq;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularSubestipulante;

public class VincularSubestipulanteApoliceHandler : IRequestHandler<VincularSubestipulanteApoliceCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public VincularSubestipulanteApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(VincularSubestipulanteApoliceCommand request, CancellationToken cancellationToken)
    {
        if (request.DataFim.HasValue && request.DataInicio.HasValue && request.DataFim < request.DataInicio)
        {
            throw new ValidacaoException("A data de fim de vigência não pode ser menor que a data de início.");
        }

        var apolice = await _dbContext.Apolices.FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId, cancellationToken);
        if (apolice == null) throw new ValidacaoException("Apólice não encontrada.");

        // Resolve SubestipulanteId directly from the DB
        var subestipulanteId = await _dbContext.Database
            .SqlQuery<long>($"SELECT id AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId} AND ativo = true")
            .FirstOrDefaultAsync(cancellationToken);

        if (subestipulanteId == 0)
        {
            // Checa se existe inativo ou se não existe
            var existeInativo = await _dbContext.Database
                .SqlQuery<bool>($"SELECT ativo AS \"Value\" FROM cadastro.subestipulante WHERE public_id = {request.SubestipulantePublicId}")
                .FirstOrDefaultAsync(cancellationToken);
            
            throw new ValidacaoException("Subestipulante não encontrado ou está inativo.");
        }

        var vinculoExistente = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(ar => ar.ApoliceId == apolice.Id && ar.SubestipulanteId == subestipulanteId, cancellationToken);
        
        if (vinculoExistente != null && vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O Subestipulante já está vinculado ativamente nesta Apólice.");
        }

        if (vinculoExistente != null && !vinculoExistente.Ativo)
        {
            throw new ValidacaoException("Existe um vínculo inativo com este Subestipulante. A reativação de vínculos ainda não está implementada/aprovada.");
        }

        var vinculo = new ApoliceSubestipulanteModel
        {
            ApoliceId = apolice.Id,
            SubestipulanteId = subestipulanteId,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.ApoliceSubestipulantes.Add(vinculo);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return vinculo.Id;
    }
}
