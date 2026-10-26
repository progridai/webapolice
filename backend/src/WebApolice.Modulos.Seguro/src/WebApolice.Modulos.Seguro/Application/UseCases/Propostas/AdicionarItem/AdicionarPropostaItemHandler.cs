using WebApolice.Modulos.Seguro.Application.Ports;
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

    private readonly IOperacaoPropostas _operacao;

    public AdicionarPropostaItemHandler(SeguroDbContext dbContext, IOperacaoPropostas operacao)
    {
        _dbContext = dbContext;
        _operacao = operacao;
    }

    public async Task<Guid> Handle(AdicionarPropostaItemCommand request, CancellationToken cancellationToken)
    {
        return await _operacao.ExecutarAsync<Guid>("PROPOSTA_ITEM_ADICIONADO", async ct =>
        {
        // 1. Carregar a Proposta
        var proposta = await _dbContext.Proposta
            .FirstOrDefaultAsync(p => p.PublicId == request.PropostaId, cancellationToken);
            
        if (proposta == null)
            throw new ValidacaoException("Proposta não encontrada.");

        // Se não tem apolice (legado solto), não tem como validar universo permitido
        if (proposta.ApoliceId == null)
            throw new ValidacaoException("Propostas legadas sem Apólice não podem receber novos itens na API atual.");

        // 2. Validação de Universo Permitido da Apólice Mãe
        // O produto DEVE estar vinculado à Apólice
        var produtoPermitido = await _dbContext.ApoliceProdutos
            .FirstOrDefaultAsync(p => p.ApoliceId == proposta.ApoliceId && p.ProdutoId == request.ProdutoId, cancellationToken);

        if (produtoPermitido == null)
            throw new ValidacaoException("O Produto selecionado não pertence ao Universo Permitido desta Apólice.");

        // O plano DEVE estar vinculado ao ProdutoPermitido da Apólice
        var planoPermitido = await _dbContext.ApolicePlanos
            .FirstOrDefaultAsync(p => p.ApoliceProdutoId == produtoPermitido.Id && p.PlanoId == request.PlanoId, cancellationToken);

        if (planoPermitido == null)
            throw new ValidacaoException("O Plano selecionado não pertence ao Produto no Universo Permitido desta Apólice.");

        // 3. Adicionar o Item na Proposta
        var item = new PropostaItem
        {
            PublicId = Guid.NewGuid(),
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

        return (item.PublicId, proposta.PublicId.ToString(), null, new { ItemPublicId = item.PublicId, proposta.PublicId });
        }, cancellationToken);
    }
}
