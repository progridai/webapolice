using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;

public class CriarPropostaHandler : IRequestHandler<CriarPropostaCommand, Guid>
{
    private readonly SeguroDbContext _dbContext;

    public CriarPropostaHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CriarPropostaCommand request, CancellationToken cancellationToken)
    {
        // 1. Carregar Entidades Mestre
        var apolice = await _dbContext.Apolices
            .FirstOrDefaultAsync(a => a.PublicId == request.ApoliceId, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("ApÃ³lice nÃ£o encontrada.");

        var apoliceVida = await _dbContext.ApoliceVidas
            .FirstOrDefaultAsync(v => v.PublicId == request.ApoliceVidaId, cancellationToken);
            
        if (apoliceVida == null)
            throw new ValidacaoException("ApoliceVida nÃ£o encontrada.");

        // 2. Travar SeguranÃ§a e Integridade Cross-Contexto
        if (apoliceVida.ApoliceId != apolice.Id)
            throw new ValidacaoException("A ApoliceVida informada nÃ£o pertence Ã  ApÃ³lice informada.");

        if (apoliceVida.ClienteId != request.ClienteId)
            throw new ValidacaoException("O Cliente informado nÃ£o Ã© o titular desta InscriÃ§Ã£o (ApoliceVida).");

        if (!apoliceVida.Ativo)
            throw new ValidacaoException("A InscriÃ§Ã£o estÃ¡ inativa e nÃ£o pode gerar nova Proposta.");

        // 3. Montar Snapshot Protegido
        var proposta = new Propostum
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ApoliceVidaId = apoliceVida.Id,
            
            // Snapshot obrigatÃ³rio de hierarquia
            EstipulanteId = apolice.EstipulanteId,
            SeguradoraId = apolice.SeguradoraId,
            SubestipulanteId = apoliceVida.ApoliceSubestipulanteId,
            
            // Snapshot HÃ­brido (Se a apÃ³lice forÃ§ar corretora, usamos ela. Se nÃ£o, permitimos o input da requisiÃ§Ã£o)
            CorretoraId = apolice.CorretoraId ?? request.CorretoraId,
            
            // VÃ­nculos FÃ­sicos (Vida)
            ClienteId = apoliceVida.ClienteId,
            ClienteVinculoId = apoliceVida.ClienteVinculoId ?? 0, 
            PessoaId = apoliceVida.ClienteId, 
            
            // Campos Financeiros/Pessoais MutÃ¡veis (Input Livre)
            ConvenioCobrancaId = request.ConvenioCobrancaId,
            ContaCobrancaId = request.ContaCobrancaId,
            PremioLiquido = request.PremioLiquido,
            ValorParcela = request.ValorParcela,
            DataPrimeiroVencimento = request.DataPrimeiroVencimento,
            DataProximoVencimento = request.DataProximoVencimento,
            BancoAgencia = request.BancoAgencia,
            BancoContaCorrente = request.BancoContaCorrente,
            
            StatusId = 1,
            Vigente = true,
            VisivelOperacional = true,
            Versao = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Proposta.Add(proposta);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return proposta.PublicId;
    }
}
