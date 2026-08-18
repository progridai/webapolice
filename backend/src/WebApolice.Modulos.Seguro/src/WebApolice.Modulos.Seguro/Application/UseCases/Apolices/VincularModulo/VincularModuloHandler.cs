using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.VincularModulo;

public class VincularModuloHandler : IRequestHandler<VincularModuloCommand, long>
{
    private readonly SeguroDbContext _dbContext;

    public VincularModuloHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<long> Handle(VincularModuloCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar se o Subestipulante pertence à Apólice informada
        var subVinculo = await _dbContext.ApoliceSubestipulantes
            .FirstOrDefaultAsync(s => s.Id == request.ApoliceSubestipulanteId && s.ApoliceId == request.ApoliceId, cancellationToken);
            
        if (subVinculo == null || !subVinculo.Ativo)
        {
            throw new ValidacaoException("Vínculo de Subestipulante inválido ou inativo para esta Apólice.");
        }

        // 2. Validar duplicidade
        var moduloExistente = await _dbContext.ApoliceSubestipulanteModulos
            .FirstOrDefaultAsync(m => m.ApoliceSubestipulanteId == request.ApoliceSubestipulanteId && m.ModuloId == request.ModuloId, cancellationToken);

        if (moduloExistente != null && moduloExistente.Ativo)
        {
            throw new ValidacaoException("Este Módulo já está vinculado a este Subestipulante ativamente nesta Apólice.");
        }

        ApoliceSubestipulanteModuloModel moduloVinculo;

        if (moduloExistente != null && !moduloExistente.Ativo)
        {
            // Reativação
            moduloExistente.Ativo = true;
            moduloExistente.UpdatedAt = DateTimeOffset.UtcNow;
            moduloVinculo = moduloExistente;
            _dbContext.ApoliceSubestipulanteModulos.Update(moduloExistente);
        }
        else
        {
            // Novo
            moduloVinculo = new ApoliceSubestipulanteModuloModel
            {
                ApoliceSubestipulanteId = request.ApoliceSubestipulanteId,
                ModuloId = request.ModuloId,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.ApoliceSubestipulanteModulos.Add(moduloVinculo);
        }

        // Adiciona rastro no Histórico
        var historico = new ApoliceHistoricoModel
        {
            ApoliceId = request.ApoliceId,
            Acao = "Vínculo Módulo",
            Descricao = $"Módulo ID {request.ModuloId} foi vinculado ao Subestipulante ID {subVinculo.SubestipulanteId} na apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _dbContext.ApoliceHistoricos.Add(historico);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return moduloVinculo.Id;
    }
}
