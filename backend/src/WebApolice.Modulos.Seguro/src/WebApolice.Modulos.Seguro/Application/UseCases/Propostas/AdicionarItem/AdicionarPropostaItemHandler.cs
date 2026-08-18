using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence;
using WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Infrastructure.Persistence.Models;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;

public class AdicionarPropostaItemHandler : IRequestHandler<AdicionarPropostaItemCommand, Guid>
{
    private readonly SeguroDbContext _dbContext;

    public AdicionarPropostaItemHandler(SeguroDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AdicionarPropostaItemCommand request, CancellationToken cancellationToken)
    {
        // 1. Carregar a Proposta
        var proposta = await _dbContext.Proposta
            .FirstOrDefaultAsync(p => p.PublicId == request.PropostaId, cancellationToken);
            
        if (proposta == null)
            throw new ValidacaoException("Proposta nÃ£o encontrada.");

        // Se nÃ£o tem apolice (legado solto), nÃ£o tem como validar universo permitido
        if (proposta.ApoliceId == null)
            throw new ValidacaoException("Propostas legadas sem ApÃ³lice nÃ£o podem receber novos itens na API atual.");

        // 2. ValidaÃ§Ã£o de Universo Permitido da ApÃ³lice MÃ£e
        // O produto DEVE estar vinculado Ã  ApÃ³lice
        var produtoPermitido = await _dbContext.ApoliceProdutos
            .FirstOrDefaultAsync(p => p.ApoliceId == proposta.ApoliceId && p.ProdutoId == request.ProdutoId, cancellationToken);

        if (produtoPermitido == null)
            throw new ValidacaoException("O Produto selecionado nÃ£o pertence ao Universo Permitido desta ApÃ³lice.");

        // O plano DEVE estar vinculado ao ProdutoPermitido da ApÃ³lice
        var planoPermitido = await _dbContext.ApolicePlanos
            .FirstOrDefaultAsync(p => p.ApoliceProdutoId == produtoPermitido.Id && p.PlanoId == request.PlanoId, cancellationToken);

        if (planoPermitido == null)
            throw new ValidacaoException("O Plano selecionado nÃ£o pertence ao Produto no Universo Permitido desta ApÃ³lice.");

        // 3. Adicionar o Item na Proposta
        var item = new PropostaItem
        {
            PropostaId = proposta.Id,
            ProdutoId = request.ProdutoId,
            PlanoId = request.PlanoId,
            Valor = request.Valor,
            Ativo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.PropostaItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Retornamos um GUID fictÃ­cio provisÃ³rio para contratos ou usamos o PublicId se existir
        return Guid.NewGuid();
    }
}
