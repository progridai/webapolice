using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarCorretora;

public sealed class CriarCorretoraHandler
{
    private readonly ICorretoraRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public CriarCorretoraHandler(
        ICorretoraRepository repository, 
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Guid> Handle(CriarCorretoraCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome da corretora é obrigatório.");

        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");
            
            // Aqui poderíamos ter uma validação CnpjJaExiste para corretoras, caso aplicável.
            // Ex: var cnpjJaExiste = await _repository.CnpjJaExisteAsync(cnpjLimpo, null, cancellationToken);
        }

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        
        try
        {
            long pessoaId;

            if (!string.IsNullOrWhiteSpace(cnpjLimpo))
            {
                var pessoaExistente = await _clienteRepository.LocalizarPessoaPorDocumentoAsync(cnpjLimpo, cancellationToken);
                
                if (pessoaExistente != null)
                {
                    if (!string.Equals(pessoaExistente.Nome, command.Nome, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("O documento informado já pertence a outra pessoa com nome divergente no sistema.");
                    }
                    pessoaId = pessoaExistente.Id;
                }
                else
                {
                    var novaPessoa = new PessoaModel(
                        2, // Pessoa Jurídica
                        command.Nome,
                        command.Cnpj,
                        cnpjLimpo,
                        true,
                        null,
                        null,
                        null
                    );
                    _clienteRepository.AdicionarPessoa(novaPessoa);
                    await _clienteRepository.SalvarAlteracoesAsync(cancellationToken);
                    pessoaId = novaPessoa.Id;
                }
            }
            else
            {
                var novaPessoa = new PessoaModel(
                    2, // Pessoa Jurídica
                    command.Nome,
                    command.Cnpj,
                    cnpjLimpo,
                    false,
                    null,
                    null,
                    null
                );
                _clienteRepository.AdicionarPessoa(novaPessoa);
                await _clienteRepository.SalvarAlteracoesAsync(cancellationToken);
                pessoaId = novaPessoa.Id;
            }
            
            var jaExiste = await _repository.CorretoraExistePorPessoaIdAsync(pessoaId, null, cancellationToken);
            if (jaExiste)
                throw new InvalidOperationException("Já existe uma corretora cadastrada para esta pessoa.");

            var corretora = new CorretoraModel
            {
                PublicId = Guid.NewGuid(),
                PessoaId = pessoaId,
                Codigo = command.Codigo,
                CodigoProtheus = command.CodigoProtheus,
                Observacao = command.Observacao,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _repository.Adicionar(corretora);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return corretora.PublicId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
