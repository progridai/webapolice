using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularSubestipulante;

public class VincularSubestipulanteHandler : IRequestHandler<VincularSubestipulanteCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public VincularSubestipulanteHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(VincularSubestipulanteCommand request, CancellationToken cancellationToken)
    {
        var apoliceExiste = await _dbContext.Apolices.AnyAsync(a => a.Id == request.ApoliceId, cancellationToken);
        if (!apoliceExiste) throw new ValidacaoException("Apólice não encontrada.");

        var vinculoExistente = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(s => s.ApoliceId == request.ApoliceId && s.SubestipulanteId == request.SubestipulanteId, cancellationToken);
        
        if (vinculoExistente != null && vinculoExistente.Ativo)
        {
            throw new ValidacaoException("O Subestipulante já está vinculado ativamente nesta Apólice.");
        }

        ApoliceSubestipulanteModel subestipulanteVinculo;
        
        if (vinculoExistente != null && !vinculoExistente.Ativo)
        {
            // Reativação
            vinculoExistente.Ativo = true;
            vinculoExistente.UpdatedAt = DateTimeOffset.UtcNow;
            subestipulanteVinculo = vinculoExistente;
            _dbContext.ApoliceSubestipulantes.Update(vinculoExistente);
        }
        else
        {
            // Novo
            subestipulanteVinculo = new ApoliceSubestipulanteModel
            {
                ApoliceId = request.ApoliceId,
                SubestipulanteId = request.SubestipulanteId,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.ApoliceSubestipulantes.Add(subestipulanteVinculo);
        }

        // Adiciona rastro no Histórico
        var historico = new ApoliceHistoricoModel
        {
            ApoliceId = request.ApoliceId,
            Acao = "Vínculo Subestipulante",
            Descricao = $"Subestipulante ID {request.SubestipulanteId} foi vinculado à apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.ApoliceHistoricos.Add(historico);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return subestipulanteVinculo.Id;
    }
}
