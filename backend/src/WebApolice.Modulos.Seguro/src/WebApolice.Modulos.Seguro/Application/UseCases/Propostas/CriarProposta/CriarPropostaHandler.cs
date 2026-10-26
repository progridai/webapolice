using Microsoft.Extensions.Options;
using WebApolice.Modulos.Cadastro.Contracts;
using WebApolice.Modulos.Seguro.Application.Ports;
using WebApolice.Modulos.Seguro.Application.UseCases.Propostas;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApolice.Modulos.Seguro.Domain.Modelos;
using WebApolice.SharedKernel.Application.Exceptions;

namespace WebApolice.Modulos.Seguro.src.WebApolice.Modulos.Seguro.Application.UseCases.Propostas.CriarProposta;

public class CriarPropostaHandler : IRequestHandler<CriarPropostaCommand, Guid>
{
    private readonly IPropostasRepository _repository;

    private readonly IClientesPropostas _clientes;
    private readonly ConfiguracaoPropostas _configuracao;
    private readonly IOperacaoPropostas _operacao;

    public CriarPropostaHandler(IPropostasRepository dbContext, IClientesPropostas clientes,
        IOptions<ConfiguracaoPropostas> configuracao, IOperacaoPropostas operacao)
    {
        _repository = dbContext;
        _clientes = clientes;
        _configuracao = configuracao.Value;
        _operacao = operacao;
    }

    public async Task<Guid> Handle(CriarPropostaCommand request, CancellationToken cancellationToken)
    {
        if (request.ClienteId <= 0)
            throw new ValidacaoException("Cliente é obrigatório.");
        if (string.IsNullOrWhiteSpace(_configuracao.StatusInicialCodigo))
            throw new ValidacaoException("O status inicial de Propostas ainda não foi definido.");
        return await _operacao.ExecutarAsync<Guid>("PROPOSTA_CRIADA", async ct =>
        {
        var status = await _repository.ObterStatusAsync(_configuracao.StatusInicialCodigo, cancellationToken)
            ?? throw new ValidacaoException("Status inicial de Propostas não encontrado no catálogo.");
        // 1. Carregar Entidades Mestre
        var apolice = await _repository.ObterApoliceAsync(request.ApoliceId, cancellationToken);
            
        if (apolice == null)
            throw new ValidacaoException("Apólice não encontrada.");

        var apoliceVida = await _repository.ObterVidaAsync(request.ApoliceVidaId, cancellationToken);
            
        if (apoliceVida == null)
            throw new ValidacaoException("ApoliceVida não encontrada.");

        // 2. Travar Segurança e Integridade Cross-Contexto
        if (apoliceVida.ApoliceId != apolice.Id)
            throw new ValidacaoException("A ApoliceVida informada não pertence à Apólice informada.");

        if (apoliceVida.ClienteId != request.ClienteId)
            throw new ValidacaoException("O Cliente informado não é o titular desta Inscrição (ApoliceVida).");

        if (!apoliceVida.Ativo)
            throw new ValidacaoException("A Inscrição está inativa e não pode gerar nova Proposta.");

        if (apoliceVida.ClienteVinculoId is not > 0)
            throw new ValidacaoException("A Vida deve possuir vínculo de Cliente/Pessoa.");
        var identidade = await _clientes.ResolverAsync(request.ClienteId, apoliceVida.ClienteVinculoId.Value, cancellationToken)
            ?? throw new ValidacaoException("Vínculo de Cliente/Pessoa inexistente, inativo ou incompatível.");

        // 3. Montar Snapshot Protegido
        var proposta = new Propostum
        {
            PublicId = Guid.NewGuid(),
            ApoliceId = apolice.Id,
            ApoliceVidaId = apoliceVida.Id,
            
            // Snapshot obrigatório de hierarquia
            EstipulanteId = apolice.EstipulanteId,
            SeguradoraId = apolice.SeguradoraId,
            
            // Snapshot Híbrido (Se a apólice forçar corretora, usamos ela. Se não, permitimos o input da requisição)
            CorretoraId = apolice.CorretoraId ?? request.CorretoraId,
            
            // Vínculos Físicos (Vida)
            ClienteId = apoliceVida.ClienteId,
            ClienteVinculoId = identidade.ClienteVinculoId,
            PessoaId = identidade.PessoaId,
            
            // Campos Financeiros/Pessoais Mutáveis (Input Livre)
            ConvenioCobrancaId = request.ConvenioCobrancaId,
            ContaCobrancaId = request.ContaCobrancaId,
            PremioLiquido = request.PremioLiquido,
            ValorParcela = request.ValorParcela,
            DataPrimeiroVencimento = request.DataPrimeiroVencimento,
            DataProximoVencimento = request.DataProximoVencimento,
            BancoAgencia = request.BancoAgencia,
            BancoContaCorrente = request.BancoContaCorrente,
            
            StatusId = status.Id,
            Vigente = true,
            VisivelOperacional = true,
            Versao = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repository.Adicionar(proposta);
        await _repository.SalvarAlteracoesAsync(cancellationToken);

        return (proposta.PublicId, proposta.PublicId.ToString(), null, new { proposta.PublicId, proposta.ApoliceVidaId, proposta.ClienteId });
        }, cancellationToken);
    }
}
