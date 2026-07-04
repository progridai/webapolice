using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;

public sealed class ConsultarClientePorIdHandler
{
    private readonly IClientesRepository _repository;

    public ConsultarClientePorIdHandler(IClientesRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConsultarClienteResult> Handle(ConsultarClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var cliente = await _repository.ObterPorIdAsync(query.Id, cancellationToken);
        if (cliente == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        var cpfMascarado = "***.***.***-" + cliente.Cpf.Substring(cliente.Cpf.Length - 2);

        return new ConsultarClienteResult(
            cliente.Id,
            cliente.Nome,
            cpfMascarado,
            cliente.DataNascimento,
            cliente.Email,
            cliente.Telefone,
            cliente.Status.ToString().ToLowerInvariant(),
            cliente.DataCadastroUtc,
            cliente.DataAtualizacaoUtc,
            cliente.CodigoLegado
        );
    }
}
