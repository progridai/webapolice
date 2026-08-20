using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSubestipulante;

public sealed class AlterarSubestipulanteHandler
{
    private readonly ISubestipulanteRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public AlterarSubestipulanteHandler(
        ISubestipulanteRepository repository,
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task Handle(AlterarSubestipulanteCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome do subestipulante é obrigatório.");

        var subestipulante = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (subestipulante == null)
            throw new InvalidOperationException("Subestipulante não encontrado.");

        var pessoa = await _clienteRepository.LocalizarPessoaPorIdAsync(subestipulante.PessoaId, cancellationToken);
        if (pessoa == null)
            throw new InvalidOperationException("Dados cadastrais do subestipulante não encontrados.");

        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");

            var cnpjJaExiste = await _repository.CnpjJaExisteAsync(cnpjLimpo, subestipulante.Id, cancellationToken);
            if (cnpjJaExiste)
                throw new InvalidOperationException("Já existe um subestipulante cadastrado com este CNPJ.");
        }

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);

        try
        {
            // Atualizar pessoa (dados comuns)
            pessoa.Nome = command.Nome;
            pessoa.DocumentoPrincipal = command.Cnpj;
            pessoa.DocumentoPrincipalLimpo = cnpjLimpo;
            pessoa.DocumentoValido = !string.IsNullOrWhiteSpace(cnpjLimpo);
            
            await _clienteRepository.SalvarAlteracoesAsync(cancellationToken);

            // Atualizar subestipulante (dados específicos)
            subestipulante.Codigo = command.Codigo;
            subestipulante.Observacao = command.Observacao;
            subestipulante.UpdatedAt = DateTimeOffset.UtcNow;

            _repository.Atualizar(subestipulante);
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
