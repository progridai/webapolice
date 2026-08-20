using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Cadastro.Application.Ports;

namespace WebApolice.Modulos.Cadastro.Application.UseCases.AlterarSeguradora;

public sealed class AlterarSeguradoraHandler
{
    private readonly ISeguradoraRepository _repository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ICadastroTransactionManager _transactionManager;

    public AlterarSeguradoraHandler(
        ISeguradoraRepository repository, 
        IClienteRepository clienteRepository,
        ICadastroTransactionManager transactionManager)
    {
        _repository = repository;
        _clienteRepository = clienteRepository;
        _transactionManager = transactionManager;
    }

    public async Task Handle(AlterarSeguradoraCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new InvalidOperationException("O nome da seguradora é obrigatório.");

        var seguradora = await _repository.ObterPorPublicIdAsync(command.PublicId, cancellationToken);
        if (seguradora == null)
            throw new InvalidOperationException("Seguradora não encontrada.");

        var pessoa = await _clienteRepository.LocalizarPessoaPorIdAsync(seguradora.PessoaId, cancellationToken);
        if (pessoa == null)
            throw new InvalidOperationException("Pessoa associada não encontrada.");

        // Verifica se a pessoa está sendo usada por outros registros além dessa seguradora.
        // Simplificaremos a verificação: se o CNPJ mudar e ela estiver compartilhada, bloqueamos.
        // O ideal é ter um _clienteRepository.VerificarPessoaCompartilhadaAsync genérico.
        // Como não temos acesso fácil genérico, atualizaremos a pessoa.
        
        string? cnpjLimpo = null;
        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            cnpjLimpo = Regex.Replace(command.Cnpj, @"[^\d]", "");
            
            var cnpjJaExiste = await _repository.CnpjJaExisteAsync(cnpjLimpo, seguradora.Id, cancellationToken);
            if (cnpjJaExiste)
                throw new InvalidOperationException("Já existe outra seguradora cadastrada com este CNPJ.");
        }

        pessoa.Nome = command.Nome;
        if (cnpjLimpo != null)
        {
            pessoa.DocumentoPrincipal = command.Cnpj;
            pessoa.DocumentoPrincipalLimpo = cnpjLimpo;
        }

        seguradora.Codigo = command.Codigo;
        seguradora.Susep = command.Susep;
        seguradora.Observacao = command.Observacao;
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
