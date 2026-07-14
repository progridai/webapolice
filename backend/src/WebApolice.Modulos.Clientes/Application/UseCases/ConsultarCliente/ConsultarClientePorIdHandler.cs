using System.Threading;
using System.Threading.Tasks;
using WebApolice.Modulos.Clientes.Application.Ports;
using WebApolice.Modulos.Clientes.Domain.Exceptions;

namespace WebApolice.Modulos.Clientes.Application.UseCases.ConsultarCliente;

public sealed class ConsultarClientePorIdHandler
{
    private readonly IClientesQueries _queries;

    public ConsultarClientePorIdHandler(IClientesQueries queries)
    {
        _queries = queries;
    }

    public async Task<ConsultarClienteResult> Handle(ConsultarClientePorIdQuery query, CancellationToken cancellationToken)
    {
        var result = await _queries.ObterDetalheAsync(query.Id, cancellationToken);
        if (result == null)
            throw new ClienteNaoEncontradoException("Cliente não encontrado.");

        return result;
    }
}
