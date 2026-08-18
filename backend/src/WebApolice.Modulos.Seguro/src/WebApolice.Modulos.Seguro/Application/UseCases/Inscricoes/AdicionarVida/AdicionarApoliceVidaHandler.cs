using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Inscricoes.AdicionarVida;

public class AdicionarApoliceVidaHandler : IRequestHandler<AdicionarApoliceVidaCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public AdicionarApoliceVidaHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(AdicionarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar se a Apólice existe
        var apoliceExiste = await _dbContext.Apolices.AnyAsync(a => a.Id == request.ApoliceId, cancellationToken);
        if (!apoliceExiste) throw new ValidacaoException("Apólice não encontrada.");

        // 2. Integridade Subestipulante
        if (request.ApoliceSubestipulanteId.HasValue)
        {
            var subVinculo = await _dbContext.ApoliceSubestipulantes
                .FirstOrDefaultAsync(s => s.Id == request.ApoliceSubestipulanteId.Value && s.ApoliceId == request.ApoliceId, cancellationToken);
            
            if (subVinculo == null || !subVinculo.Ativo)
            {
                throw new ValidacaoException("O Subestipulante informado não pertence a esta apólice ou está inativo.");
            }

            // 3. Integridade Módulo
            if (request.ApoliceSubestipulanteModuloId.HasValue)
            {
                var moduloVinculo = await _dbContext.ApoliceSubestipulanteModulos
                    .FirstOrDefaultAsync(m => m.Id == request.ApoliceSubestipulanteModuloId.Value 
                                         && m.ApoliceSubestipulanteId == request.ApoliceSubestipulanteId.Value, cancellationToken);

                if (moduloVinculo == null || !moduloVinculo.Ativo)
                {
                    throw new ValidacaoException("O Módulo informado não pertence ao Subestipulante desta apólice ou está inativo.");
                }
            }
        }
        else if (request.ApoliceSubestipulanteModuloId.HasValue)
        {
            // Regra de Ouro: Módulo nunca pode vir sozinho
            throw new ValidacaoException("Não é permitido inscrever uma Vida em um Módulo sem informar o Subestipulante.");
        }

        var vida = new ApoliceVidaModel
        {
            ApoliceId = request.ApoliceId,
            ApoliceSubestipulanteId = request.ApoliceSubestipulanteId,
            ApoliceSubestipulanteModuloId = request.ApoliceSubestipulanteModuloId,
            ClienteId = request.ClienteId,
            DataInicioVigencia = DateOnly.FromDateTime(request.DataInclusao),
            Ativo = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ApoliceVidas.Add(vida);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return vida.Id;
    }
}
