using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Apolices.InativarApoliceVida;

/// <summary>
/// Handler para encerrar (inativar) a participação de um Cliente (Vida) em uma Apólice.
///
/// Regras de negócio:
/// 1. Apólice deve existir.
/// 2. Vida deve existir e pertencer à Apólice.
/// 3. Vida já encerrada (ativo=false) retorna erro (idempotência explícita).
/// 4. Encerramento semântico: ativo=false, status='encerrada'.
///    IMPORTANTE: deleted_at permanece NULL — encerramento é funcional, não exclusão técnica.
/// 5. Registrar Histórico funcional da Apólice.
///
/// Nova participação posterior é permitida — basta criar um novo registro (POST).
/// </summary>
public class InativarApoliceVidaHandler
{
    private readonly SeguroDbContext _dbContext;

    public InativarApoliceVidaHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(InativarApoliceVidaCommand request, CancellationToken cancellationToken)
    {
        // 1. Localizar Apólice
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApolicePublicId && a.DeletedAt == null, cancellationToken);
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        // 2. Localizar Vida (deleted_at IS NULL — registros com deleted_at preenchido são excluídos tecnicamente)
        var vida = await _dbContext.ApoliceVidas
            .FirstOrDefaultAsync(v =>
                v.PublicId == request.ApoliceVidaPublicId &&
                v.ApoliceId == apolice.Id &&
                v.DeletedAt == null, cancellationToken);

        if (vida == null)
            throw new ValidacaoException("Vida não encontrada nesta Apólice.");

        // 3. Idempotência explícita
        if (!vida.Ativo)
            throw new ValidacaoException("Esta participação já está encerrada.");

        // 4. Encerramento funcional — NÃO usar deleted_at
        vida.Ativo = false;
        vida.Status = "encerrada";
        vida.UpdatedAt = DateTimeOffset.UtcNow;
        // deleted_at permanece NULL

        // 5. Registrar Histórico funcional
        _dbContext.ApoliceHistoricos.Add(new ApoliceHistoricoModel
        {
            ApoliceId = apolice.Id,
            Acao = "Encerramento Vida",
            Descricao = $"Participação de Vida (PublicId: {request.ApoliceVidaPublicId}) encerrada na Apólice.",
            UsuarioPublicId = request.UsuarioPublicId,
            DataAcao = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
