using System;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.InativarSeguradora;

public sealed class InativarSeguradoraHandler
{
    private readonly ISeguradoraRepository _repository;
    private readonly ICadastroTransactionManager _transactionManager;

    public InativarSeguradoraHandler(ISeguradoraRepository repository, ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _transactionManager = transactionManager;
    }

    public async Task Handle(InativarSeguradoraCommand command, CancellationToken cancellationToken)
    {
        var seguradora = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (seguradora == null)
            throw new InvalidOperationException("Seguradora não encontrada.");

        if (!seguradora.Ativo)
            return;

        seguradora.Ativo = false;
        seguradora.UpdatedAt = DateTimeOffset.UtcNow;

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _repository.Atualizar(seguradora);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
