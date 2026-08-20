using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarSeguradora;

public sealed class CriarSeguradoraHandler
{
    private readonly ISeguradoraRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public CriarSeguradoraHandler(
        ISeguradoraRepository repository, 
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Guid> Handle(CriarSeguradoraCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome da seguradora é obrigatório.");

        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");
            
            var cnpjJaExiste = await _repository.CnpjJaExisteAsync(cnpjLimpo, null, cancellationToken);
            if (cnpjJaExiste)
                throw new InvalidOperationException("Já existe uma seguradora cadastrada com este CNPJ.");
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
                    // Verifica se o nome da pessoa existente diverge muito do nome informado?
                    // Conforme combinado com o cliente, utilizaremos a mesma lógica de Cliente (verificando divergência simples).
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
                        true, // documento_valido (simplificado)
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
                // Seguradora sem CNPJ? (O campo era opcional, vamos permitir pessoa sem documento para compatibilidade se command.Cnpj for null)
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

            var seguradora = new SeguradoraModel
            {
                PublicId = Guid.NewGuid(),
                PessoaId = pessoaId,
                Codigo = command.Codigo,
                Susep = command.Susep,
                Observacao = command.Observacao,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _repository.Adicionar(seguradora);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return seguradora.PublicId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
