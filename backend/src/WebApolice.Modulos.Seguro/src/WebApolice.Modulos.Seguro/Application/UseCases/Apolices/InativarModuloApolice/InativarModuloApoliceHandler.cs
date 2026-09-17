using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarModuloApolice;

public class InativarModuloApoliceHandler : IRequestHandler<InativarModuloApoliceCommand, Unit>
{
    private readonly SeguroDbContext _dbContext;

    public InativarModuloApoliceHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(InativarModuloApoliceCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");
            
        // 2. Localizar Vínculo e garantir isolamento
        var vinculo = await _dbContext.ApoliceModulos
            .FirstOrDefaultAsync(m => m.PublicId == request.ApoliceModuloPublicId && m.DeletedAt == null, cancellationToken);

        if (vinculo == null)
            throw new ValidacaoException("Vínculo do Módulo não encontrado.");

        if (vinculo.ApoliceId != apolice.Id)
            throw new ValidacaoException("O Vínculo informado não pertence à Apólice especificada.");

        if (!vinculo.Ativo)
            throw new ValidacaoException("O Vínculo do Módulo já está inativo.");

        // 3. Inativar Vínculo
        vinculo.Ativo = false;
        vinculo.UpdatedAt = DateTimeOffset.UtcNow;

        // 4. Histórico
        // Buscar nome do Módulo
        var moduloNome = await _dbContext.Database
            .SqlQuery<string>($"SELECT nome AS \"Value\" FROM cadastro.modulo WHERE id = {vinculo.ModuloId}")
            .FirstOrDefaultAsync(cancellationToken);

        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Inativação de Vínculo de Módulo",
            Descricao = $"Vínculo do Módulo '{moduloNome}' inativado na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
