using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence;
using WebApolice.Modulos.Clientes.Infrastructure.Persistence.Models;

namespace WebApolice.Modulos.Clientes.Application.UseCases.AlterarCliente;

public sealed class AlterarClienteHandler
{
    private readonly IClienteRepository _repository;
    private readonly ClientesDbContext _dbContext;

    public AlterarClienteHandler(IClienteRepository repository, ClientesDbContext dbContext)
    {
        _repository = repository;
        _dbContext = dbContext;
    }

    public async Task Handle(AlterarClienteCommand command, string usuarioSub, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Nome))
            throw new ClienteInvalidoException("O nome é obrigatório.");

        if (!command.DataNascimento.HasValue)
            throw new ClienteInvalidoException("A data de nascimento é obrigatória.");

        if (command.Falecido && !command.DataObito.HasValue)
            throw new ClienteInvalidoException("Data de óbito é obrigatória para clientes falecidos.");

        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var cliente = await _repository.ObterParaEdicaoPorPublicIdAsync(command.Id, cancellationToken);
            if (cliente == null)
                throw new ClienteNaoEncontradoException("Cliente não encontrado ou excluído.");

            var pessoa = await _repository.LocalizarPessoaPorIdAsync(cliente.PessoaId, cancellationToken);
            if (pessoa == null)
                throw new ClienteNaoEncontradoException("Pessoa associada não encontrada.");

            // Verifica se a pessoa está compartilhada com outros papéis
            var compartilhada = await _repository.VerificarPessoaCompartilhadaAsync(pessoa.Id, cliente.Id, cancellationToken);
            if (compartilhada)
                throw new ClienteInvalidoException("Os dados dessa pessoa são compartilhados com outros papéis no sistema e não podem ser alterados diretamente por aqui.");

            // Atualiza Dados Pessoais e Cliente
            pessoa.AtualizarDadosPessoais(command.Nome, command.DataNascimento, command.Sexo, command.Observacao);
            cliente.AtualizarDados(command.Falecido, command.DataObito, command.Observacao);

            // Contatos
            await TratarContato(pessoa.Id, "EMAIL", command.Email, cancellationToken);
            await TratarContato(pessoa.Id, "TELEFONE", command.Telefone, cancellationToken);
            await TratarContato(pessoa.Id, "CELULAR", command.Celular, cancellationToken);

            // Endereço
            var enderecoAtual = await _repository.ObterEnderecoPrincipalAsync(pessoa.Id, cancellationToken);
            if (command.Endereco != null)
            {
                if (enderecoAtual != null)
                {
                    enderecoAtual.AtualizarEndereco(
                        command.Endereco.CidadeId,
                        command.Endereco.Cep,
                        command.Endereco.Logradouro,
                        command.Endereco.Numero,
                        command.Endereco.Complemento,
                        command.Endereco.Bairro,
                        command.Endereco.Uf,
                        true
                    );
                }
                else
                {
                    _repository.AdicionarEndereco(new PessoaEnderecoModel(
                        pessoa.Id,
                        command.Endereco.CidadeId,
                        "RESIDENCIAL",
                        command.Endereco.Cep,
                        command.Endereco.Logradouro,
                        command.Endereco.Numero,
                        command.Endereco.Complemento,
                        command.Endereco.Bairro,
                        command.Endereco.Uf,
                        true
                    ));
                }
            }

            await _repository.SalvarAlteracoesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task TratarContato(long pessoaId, string tipoContato, string? valor, CancellationToken cancellationToken)
    {
        var contatoAtual = await _repository.ObterContatoPrincipalAsync(pessoaId, tipoContato, cancellationToken);

        if (string.IsNullOrWhiteSpace(valor))
        {
            contatoAtual?.Inativar();
        }
        else
        {
            var valorLimpo = tipoContato != "EMAIL" ? LimparDocumento(valor) : valor.ToUpperInvariant();
            
            if (contatoAtual != null)
            {
                contatoAtual.AtualizarValor(valor, valorLimpo, true);
            }
            else
            {
                _repository.AdicionarContato(new PessoaContatoModel(pessoaId, tipoContato, valor, valorLimpo, true));
            }
        }
    }

    private static string LimparDocumento(string? doc)
    {
        if (string.IsNullOrWhiteSpace(doc)) return "";
        return new string(doc.Where(char.IsDigit).ToArray());
    }
}
