using WebApolice.Modulos.Seguro.Application.Ports;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.AdicionarItem;

public class AdicionarPropostaItemHandler : IRequestHandler<AdicionarPropostaItemCommand, Guid>
{
    private readonly IPropostasRepository _repository;

    private readonly IOperacaoPropostas _operacao;

    public AdicionarPropostaItemHandler(IPropostasRepository dbContext, IOperacaoPropostas operacao)
    {
        _repository = dbContext;
        _operacao = operacao;
    }

    public async Task<Guid> Handle(AdicionarPropostaItemCommand request, CancellationToken cancellationToken)
    {
        return await _operacao.ExecutarAsync<Guid>("PROPOSTA_ITEM_ADICIONADO", async ct =>
        {
        // 1. Carregar a Proposta
        var proposta = await _repository.ObterAsync(request.PropostaId, cancellationToken);
            
        if (proposta == null)
            throw new ValidacaoException("Proposta não encontrada.");

        // Se não tem apolice (legado solto), não tem como validar universo permitido
        if (proposta.ApoliceId == null)
            throw new ValidacaoException("Propostas legadas sem Apólice não podem receber novos itens na API atual.");

        // 2. Validação de Universo Permitido da Apólice Mãe
        // O produto DEVE estar vinculado à Apólice
        var produtoPermitido = await _repository.ObterProdutoPermitidoAsync(proposta.ApoliceId, request.ProdutoId, cancellationToken);

        if (produtoPermitido == null)
            throw new ValidacaoException("O Produto selecionado não pertence ao Universo Permitido desta Apólice.");

        // O plano DEVE estar vinculado ao ProdutoPermitido da Apólice
        var planoPermitido = await _repository.ObterPlanoPermitidoAsync(produtoPermitido.Id, request.PlanoId, cancellationToken);

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

        _repository.AdicionarItem(item);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return (item.PublicId, proposta.PublicId.ToString(), null, new { ItemPublicId = item.PublicId, proposta.PublicId });
        }, cancellationToken);
    }
}
