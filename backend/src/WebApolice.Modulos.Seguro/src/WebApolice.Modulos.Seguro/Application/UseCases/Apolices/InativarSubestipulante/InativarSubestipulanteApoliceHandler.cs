using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.SharedKernel.Application.Exceptions;
using System.Linq;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarSubestipulante;

public class InativarSubestipulanteApoliceHandler : IRequestHandler<InativarSubestipulanteApoliceCommand>
{
    private readonly SeguroDbContext _dbContext;

    public InativarSubestipulanteApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(InativarSubestipulanteApoliceCommand request, CancellationToken cancellationToken)
    {
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
            .Include(v => v.Vidas)
            .Include(v => v.Modulos)
            .FirstOrDefaultAsync(ar => ar.ApoliceId == apolice.Id && ar.SubestipulanteId == subestipulanteId, cancellationToken);
        
        if (vinculoExistente == null)
        {
            throw new ValidacaoException("Vínculo não encontrado.");
        }

        if (!vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O vínculo já está inativo.");
        }

        // Validação de dependências ativas
        if (vinculoExistente.Vidas.Any(v => v.Ativo))
        {
            throw new ValidacaoException("Não é possível inativar o vínculo pois existem Vidas ativas associadas a este Subestipulante.");
        }

        if (vinculoExistente.Modulos.Any(m => m.Ativo))
        {
            throw new ValidacaoException("Não é possível inativar o vínculo pois existem Módulos ativos associados a este Subestipulante.");
        }

        vinculoExistente.Ativo = false;
        vinculoExistente.UpdatedAt = DateTimeOffset.UtcNow;

        _dbContext.ApoliceSubestipulantes.Update(vinculoExistente);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
