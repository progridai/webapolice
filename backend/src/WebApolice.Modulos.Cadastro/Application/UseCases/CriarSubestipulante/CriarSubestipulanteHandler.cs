using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models;
using WebApolice.Modulos.Cadastro.Infrastructure.Persistence.Models.Vinculos;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.CriarSubestipulante;

public sealed class CriarSubestipulanteHandler
{
    private readonly ISubestipulanteRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public CriarSubestipulanteHandler(
        ISubestipulanteRepository repository,
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Guid> Handle(CriarSubestipulanteCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome do subestipulante é obrigatório.");

        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");

            var cnpjJaExiste = await _repository.CnpjJaExisteAsync(cnpjLimpo, null, cancellationToken);
            if (cnpjJaExiste)
                throw new InvalidOperationException("Já existe um subestipulante cadastrado com este CNPJ.");
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
                        throw new InvalidOperationException("O CNPJ informado já pertence a outra pessoa com nome divergente no sistema.");

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
                // Subestipulante sem CNPJ (compatibilidade com legado)
                var novaPessoa = new PessoaModel(
                    2, // Pessoa Jurídica
                    command.Nome,
                    null,
                    null,
                    false,
                    null,
                    null,
                    null
                );
                _clienteRepository.AdicionarPessoa(novaPessoa);
                await _clienteRepository.SalvarAlteracoesAsync(cancellationToken);
                pessoaId = novaPessoa.Id;
            }

            var subestipulante = new SubestipulanteModel
            {
                PublicId = Guid.NewGuid(),
                PessoaId = pessoaId,
                Codigo = command.Codigo,
                Observacao = command.Observacao,
                Ativo = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _repository.Adicionar(subestipulante);
            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return subestipulante.PublicId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
