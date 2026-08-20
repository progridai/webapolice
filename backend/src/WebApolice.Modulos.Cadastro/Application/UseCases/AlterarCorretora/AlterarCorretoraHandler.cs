using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarCorretora;

public sealed class AlterarCorretoraHandler
{
    private readonly ICorretoraRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public AlterarCorretoraHandler(
        ICorretoraRepository repository, 
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task Handle(AlterarCorretoraCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome da corretora é obrigatório.");

        var corretora = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (corretora == null)
            throw new InvalidOperationException("Corretora não encontrada.");

        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");
            
            if (cnpjLimpo != corretora.Pessoa.DocumentoPrincipalLimpo)
            {
                var pessoaExistente = await _clienteRepository.LocalizarPessoaPorDocumentoAsync(cnpjLimpo, cancellationToken);
                if (pessoaExistente != null && pessoaExistente.Id != corretora.PessoaId)
                {
                    throw new InvalidOperationException("O CNPJ informado já pertence a outra pessoa.");
                }
            }
        }

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Atualizar Pessoa
            var pessoa = corretora.Pessoa;
            pessoa.Nome = command.Nome;
            pessoa.DocumentoPrincipal = command.Cnpj;
            pessoa.DocumentoPrincipalLimpo = cnpjLimpo;
            pessoa.DocumentoValido = !string.IsNullOrWhiteSpace(cnpjLimpo);

            // Atualizar Corretora
            corretora.Codigo = command.Codigo;
            corretora.CodigoProtheus = command.CodigoProtheus;
            corretora.Observacao = command.Observacao;
            corretora.UpdatedAt = DateTimeOffset.UtcNow;

            _repository.Atualizar(corretora);
            
            await _clienteRepository.SalvarAlteracoesAsync(cancellationToken);
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
